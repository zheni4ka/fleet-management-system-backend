using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using transport_logistic_management_2026.Constants;
using transport_logistic_management_2026.Options;

namespace transport_logistic_management_2026.Helpers
{
    public static class ObservabilityServiceExtensions
    {
        private const string EntityFrameworkCoreMeterName = "Microsoft.EntityFrameworkCore";
        private const string HealthCheckPathPrefix = "/health";

        public static WebApplicationBuilder AddConfiguredObservability(this WebApplicationBuilder builder)
        {
            builder.Services.AddValidatedSettings<ObservabilitySettings, ObservabilitySettingsValidator>(builder.Configuration, ObservabilitySettings.SectionName);
            var observabilitySettings = builder.Configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>()
                ?? throw new InvalidOperationException($"'{ObservabilitySettings.SectionName}' is not configured.");
            var observabilitySettingsValidationResult = new ObservabilitySettingsValidator().Validate(null, observabilitySettings);
            if (observabilitySettingsValidationResult.Failed)
            {
                throw new OptionsValidationException(
                    ObservabilitySettings.SectionName,
                    typeof(ObservabilitySettings),
                    observabilitySettingsValidationResult.Failures);
            }

            builder.Services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.FromLogContext()
                .WriteTo.Console(new RenderedCompactJsonFormatter()));

            var isOtlpExporterDisabled = string.IsNullOrWhiteSpace(observabilitySettings.OtlpEndpoint);
            if (isOtlpExporterDisabled)
            {
                return builder;
            }

            var otlpEndpoint = new Uri(observabilitySettings.OtlpEndpoint);
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService(observabilitySettings.ServiceName))
                .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                    .AddAspNetCoreInstrumentation(aspNetCoreTraceOptions =>
                    {
                        aspNetCoreTraceOptions.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments(HealthCheckPathPrefix);
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(otlpExporterOptions => otlpExporterOptions.Endpoint = otlpEndpoint))
                .WithMetrics(meterProviderBuilder => meterProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(EntityFrameworkCoreMeterName)
                    .AddOtlpExporter(otlpExporterOptions => otlpExporterOptions.Endpoint = otlpEndpoint));

            return builder;
        }

        public static IServiceCollection AddDatabaseHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<HealthChecks.DatabaseReadinessHealthCheck>(
                    HealthCheckEndpoints.DatabaseCheckName,
                    tags: [HealthCheckEndpoints.ReadinessTag]);
            return services;
        }
    }
}
