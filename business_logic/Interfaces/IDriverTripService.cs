using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IDriverTripService
    {
        Task<GetPagedListResponseModel<GetDriverTripResponseModel>> GetPagedListAsync(
            GetDriverTripsRequestModel driverTripsRequest,
            GetPagedListRequestModel<DriverTripSortField> pagedListRequest);
        Task<GetDriverTripSummaryResponseModel> GetSummaryAsync(int driverId);
        Task RecordFinishedTripAsync(Route finishedRoute, Auto routeAuto);
    }
}
