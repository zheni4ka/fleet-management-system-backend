using Microsoft.Extensions.Options;

namespace transport_logistic_management_2026.Helpers
{
    public static class SettingsServiceExtensions
    {
        public static IServiceCollection AddValidatedSettings<TSettings, TSettingsValidator>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where TSettings : class
            where TSettingsValidator : class, IValidateOptions<TSettings>
        {
            services.AddSingleton<IValidateOptions<TSettings>, TSettingsValidator>();
            return services.AddValidatedSettings<TSettings>(configuration, sectionName);
        }

        public static IServiceCollection AddValidatedSettings<TSettings>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where TSettings : class
        {
            services.AddOptions<TSettings>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateOnStart();
            return services;
        }
    }
}
