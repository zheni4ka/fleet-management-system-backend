using business_logic;
using business_logic.Interfaces;
using data_access;
using data_access.Identity;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using transport_logistic_management_2026.Helpers;
using transport_logistic_management_2026.Services;

namespace transport_logistic_management_2026
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddBusinessLogicServices();
            builder.Services.AddDataAccessServices(connectionString);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowNextJS", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<Middlewares.GlobalExceptionHandler>();

            var app = builder.Build();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowNextJS");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await IdentitySeeder.SeedAsync(app.Services);

            await app.RunAsync();
        }
    }
}
