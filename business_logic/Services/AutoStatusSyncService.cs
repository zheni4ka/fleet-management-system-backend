using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Specifications;

namespace business_logic.Services
{
    public class AutoStatusSyncService : IAutoStatusSyncService
    {
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IRepository<AutoMaintenance> _maintenanceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AutoStatusSyncService(
            IRepository<Auto> autoRepository,
            IRepository<Route> routeRepository,
            IRepository<AutoMaintenance> maintenanceRepository,
            IUnitOfWork unitOfWork)
        {
            _autoRepository = autoRepository;
            _routeRepository = routeRepository;
            _maintenanceRepository = maintenanceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task SyncAutoStatusAsync(Auto auto)
        {
            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.InProgressByAutoId(auto.Id)))
            {
                auto.Status = AutoStatus.InService;
                return;
            }

            var hasIncompleteDueMaintenance = await _maintenanceRepository.AnyBySpecAsync(
                new AutoMaintenanceSpecs.IncompleteDueByAutoId(auto.Id, GetDueMaintenanceEndExclusiveDateTime()));
            auto.Status = hasIncompleteDueMaintenance ? AutoStatus.UnderMaintenance : AutoStatus.Available;
        }

        public async Task SyncAutoStatusAsync(int autoId)
        {
            var auto = await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(autoId)) ?? throw NotFoundException.For<Auto>(autoId);
            await SyncAutoStatusAsync(auto);
        }

        public async Task<int> SyncAutosWithDueMaintenanceAsync()
        {
            var autosAwaitingUnderMaintenanceStatus = (await _autoRepository.GetListBySpecAsync(
                new AutoSpecs.AwaitingUnderMaintenanceStatusByDueMaintenance(GetDueMaintenanceEndExclusiveDateTime()))).ToList();

            foreach (var auto in autosAwaitingUnderMaintenanceStatus)
            {
                auto.Status = AutoStatus.UnderMaintenance;
            }

            await _unitOfWork.SaveChangesAsync();
            return autosAwaitingUnderMaintenanceStatus.Count;
        }

        private static DateTime GetDueMaintenanceEndExclusiveDateTime()
        {
            return BusinessTimeZoneConverter.GetBusinessToday().AddDays(1).ToDateTime(TimeOnly.MinValue);
        }
    }
}
