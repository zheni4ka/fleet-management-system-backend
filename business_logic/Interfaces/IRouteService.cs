using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IRouteService
    {
        Task<IEnumerable<RouteDTO>> GetAllAsync();
        Task<RouteDTO> GetAsync(int id);
        Task<IEnumerable<RouteDTO>> GetRoutesBySearchFilterAsync(GetRoutesBySearchFilterRequestModel searchFilter);
        Task<GetPagedListResponseModel<RouteDTO>> GetRoutesPagedListBySearchFilterAsync(
            GetRoutesBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<RouteSortField> pagedListRequest);
        Task<GetRoutesPeriodReportResponseModel> GetRoutesPeriodReportAsync(GetRoutesPeriodReportRequestModel periodReportRequest);
        Task<RouteDTO> CreateAsync(CreateRouteModel model);
        Task UpdateAsync(EditRouteModel model);
        Task DeleteAsync(int id);
    }
}
