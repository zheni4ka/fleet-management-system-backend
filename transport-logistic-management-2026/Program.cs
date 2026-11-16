using Asp.Versioning;
using business_logic;
using business_logic.Constants;
using business_logic.Interfaces;
using business_logic.Settings;
using data_access;
using data_access.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using transport_logistic_management_2026.Constants;
using transport_logistic_management_2026.Helpers;
using transport_logistic_management_2026.Services;

namespace transport_logistic_management_2026
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (!builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>(optional: true);
            }

            var connectionString = builder.Configuration.GetConnectionString("connectionString")
                ?? throw new InvalidOperationException("Connection string 'connectionString' is not configured.");

            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddApiVersioning(apiVersioningOptions =>
            {
                apiVersioningOptions.DefaultApiVersion = new ApiVersion(ApiVersions.Version1MajorVersion);
                apiVersioningOptions.AssumeDefaultVersionWhenUnspecified = false;
                apiVersioningOptions.ReportApiVersions = true;
            }).AddMvc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddBusinessLogicServices();
            builder.Services.AddValidatedSettings<RefreshTokenSettings>(builder.Configuration, RefreshTokenSettings.SectionName);
            builder.Services.AddValidatedSettings<ExternalIdentitySettings>(builder.Configuration, ExternalIdentitySettings.SectionName);
            builder.Services.AddValidatedSettings<AuditLogRetentionSettings>(builder.Configuration, AuditLogRetentionSettings.SectionName);
            builder.Services.AddSingleton<IExternalIdentityTokenValidator, EntraIdTokenValidator>();
            builder.Services.AddDataAccessServices(connectionString);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddHostedService<DueMaintenanceAutoStatusSyncBackgroundService>();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddConfiguredForwardedHeaders(builder.Configuration);
            builder.Services.AddConfiguredCors(builder.Configuration);
            builder.Services.AddApiRateLimiting(builder.Configuration);

            builder.Services.AddDatabaseHealthChecks();
            builder.AddConfiguredObservability();

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<Middlewares.GlobalExceptionHandler>();

            var app = builder.Build();

            app.UseForwardedHeaders();
            app.UseRequestLocalization(requestLocalizationOptions =>
            {
                requestLocalizationOptions.SetDefaultCulture(SupportedLanguages.Default)
                    .AddSupportedCultures([.. SupportedLanguages.All])
                    .AddSupportedUICultures([.. SupportedLanguages.All]);
                requestLocalizationOptions.ApplyCurrentCultureToResponseHeaders = true;
                requestLocalizationOptions.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
            });
            app.UseSerilogRequestLogging();
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors(CorsPolicyNames.Frontend);

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks(HealthCheckEndpoints.LivenessPath, new HealthCheckOptions { Predicate = _ => false })
                .DisableRateLimiting();
            app.MapHealthChecks(HealthCheckEndpoints.ReadinessPath, new HealthCheckOptions
            {
                Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains(HealthCheckEndpoints.ReadinessTag)
            })
                .DisableRateLimiting();

            await IdentitySeeder.SeedAsync(
                app.Services,
                app.Configuration.GetSection(SeedAdminSettings.SectionName).Get<SeedAdminSettings>());

            await app.RunAsync();
        }
    }
}
