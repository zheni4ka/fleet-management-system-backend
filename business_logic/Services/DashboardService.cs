using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Specifications;

namespace business_logic.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Route> _routeRepository;

        public DashboardService(
            IRepository<Auto> autoRepository,
            IRepository<Driver> driverRepository,
            IRepository<Route> routeRepository)
        {
            _autoRepository = autoRepository;
            _driverRepository = driverRepository;
            _routeRepository = routeRepository;
        }

        public async Task<GetDashboardSummaryResponseModel> GetDashboardSummaryAsync()
        {
            var autosCount = await _autoRepository.CountBySpecAsync(new AutoSpecs.Unarchived());
            var driversCount = await _driverRepository.CountBySpecAsync(new DriverSpecs.Unarchived());
            var routesCount = await _routeRepository.CountBySpecAsync(new RouteSpecs.All());
            var activeRoutesCount = await _routeRepository.CountBySpecAsync(new RouteSpecs.Active());

            return new GetDashboardSummaryResponseModel(
                AutosCount: autosCount,
                DriversCount: driversCount,
                RoutesCount: routesCount,
                ActiveRoutesCount: activeRoutesCount);
        }
    }
}
