using business_logic.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using transport_logistic_management_2026.Constants;
using transport_logistic_management_2026.Options;

namespace transport_logistic_management_2026.Helpers
{
    public static class RateLimitingServiceExtensions
    {
        private const string ProblemJsonContentType = "application/problem+json";

        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatedSettings<RateLimitingSettings, RateLimitingSettingsValidator>(configuration, RateLimitingSettings.SectionName);
            services.AddRateLimiter(_ => { });

            services.AddOptions<RateLimiterOptions>()
                .Configure<IOptions<RateLimitingSettings>>((rateLimiterOptions, rateLimitingSettingsOptions) =>
                {
                    var rateLimitingSettings = rateLimitingSettingsOptions.Value;
                    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    rateLimiterOptions.OnRejected = WriteTooManyRequestsProblemAsync;

                    rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            GetUserOrIpAddressPartitionKey(httpContext),
                            _ => CreateFixedWindowRateLimiterOptions(
                                rateLimitingSettings.GeneralPermitLimitPerUser,
                                rateLimitingSettings.GeneralWindowInSeconds)));

                    rateLimiterOptions.AddPolicy(RateLimitingPolicyNames.CredentialEndpointsPerIpAddress, httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            GetIpAddressPartitionKey(httpContext),
                            _ => CreateFixedWindowRateLimiterOptions(
                                rateLimitingSettings.CredentialEndpointsPermitLimitPerIpAddress,
                                rateLimitingSettings.CredentialEndpointsWindowInSeconds)));
                });

            return services;
        }

        private static FixedWindowRateLimiterOptions CreateFixedWindowRateLimiterOptions(int permitLimit, int windowInSeconds)
        {
            return new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowInSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            };
        }

        private static string GetUserOrIpAddressPartitionKey(HttpContext httpContext)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                return RateLimitingPartitionKeys.UserPrefix + userId;
            }

            return GetIpAddressPartitionKey(httpContext);
        }

        private static string GetIpAddressPartitionKey(HttpContext httpContext)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress is null)
            {
                return RateLimitingPartitionKeys.UnknownRemoteIpAddress;
            }

            return RateLimitingPartitionKeys.IpAddressPrefix + remoteIpAddress;
        }

        private static async ValueTask WriteTooManyRequestsProblemAsync(OnRejectedContext rejectedContext, CancellationToken cancellationToken)
        {
            var httpResponse = rejectedContext.HttpContext.Response;
            if (rejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                httpResponse.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
            }

            var tooManyRequestsProblem = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = ExceptionMessages.TooManyRequests,
                Instance = rejectedContext.HttpContext.Request.Path
            };
            await httpResponse.WriteAsJsonAsync(tooManyRequestsProblem, options: null, contentType: ProblemJsonContentType, cancellationToken);
        }
    }
}
