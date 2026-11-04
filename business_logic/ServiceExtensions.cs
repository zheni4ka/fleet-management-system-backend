using business_logic.Interfaces;
using business_logic.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace business_logic
{
    public static class ServiceExtensions
    {
        public static void AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ApplicationProfile).Assembly);
            });

            services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly);

            services.AddCustomServices();
        }

        private static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IAutoService, AutoService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IAutoMaintenanceService, AutoMaintenanceService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
        }
    }
}
