using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Net;
using transport_logistic_management_2026.Constants;
using transport_logistic_management_2026.Options;

namespace transport_logistic_management_2026.Helpers
{
    public static class NetworkServiceExtensions
    {
        public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatedSettings<CorsSettings, CorsSettingsValidator>(configuration, CorsSettings.SectionName);
            services.AddCors();

            services.AddOptions<CorsOptions>()
                .Configure<IOptions<CorsSettings>>((corsOptions, corsSettingsOptions) =>
                {
                    corsOptions.AddPolicy(CorsPolicyNames.Frontend, corsPolicyBuilder =>
                    {
                        corsPolicyBuilder.WithOrigins(corsSettingsOptions.Value.AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });

            return services;
        }

        public static IServiceCollection AddConfiguredForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatedSettings<ForwardedHeadersSettings, ForwardedHeadersSettingsValidator>(configuration, ForwardedHeadersSettings.SectionName);

            services.AddOptions<ForwardedHeadersOptions>()
                .Configure<IOptions<ForwardedHeadersSettings>>((forwardedHeadersOptions, forwardedHeadersSettingsOptions) =>
                {
                    forwardedHeadersOptions.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    foreach (var knownProxyAddress in forwardedHeadersSettingsOptions.Value.KnownProxyAddresses)
                    {
                        forwardedHeadersOptions.KnownProxies.Add(IPAddress.Parse(knownProxyAddress));
                    }
                });

            return services;
        }
    }
}
