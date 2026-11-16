using business_logic.DTOs;
using business_logic.Interfaces;
using business_logic.Services;
using business_logic.Settings;
using business_logic.Validators.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            services.AddPagedListRequestValidators();

            services.AddCustomServices();
        }

        private static void AddPagedListRequestValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<GetPagedListRequestModel<AutoSortField>>, GetPagedListRequestModelValidator<AutoSortField>>();
            services.AddScoped<IValidator<GetPagedListRequestModel<DriverSortField>>, GetPagedListRequestModelValidator<DriverSortField>>();
            services.AddScoped<IValidator<GetPagedListRequestModel<LocationSortField>>, GetPagedListRequestModelValidator<LocationSortField>>();
            services.AddScoped<IValidator<GetPagedListRequestModel<DispatcherSortField>>, GetPagedListRequestModelValidator<DispatcherSortField>>();
            services.AddScoped<IValidator<GetPagedListRequestModel<RouteSortField>>, GetPagedListRequestModelValidator<RouteSortField>>();
            services.AddScoped<IValidator<GetPagedListRequestModel<AuditLogSortField>>, GetPagedListRequestModelValidator<AuditLogSortField>>();
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
            services.AddScoped<IAutoStatusSyncService, AutoStatusSyncService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddSingleton<IValidateOptions<RefreshTokenSettings>, RefreshTokenSettingsValidator>();
        }
    }
}
