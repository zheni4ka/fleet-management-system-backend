using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IRouteService
    {
        Task<GetRouteResponseModel> GetAsync(int id);
        Task<GetPagedListResponseModel<GetRouteResponseModel>> GetRoutesPagedListBySearchFilterAsync(
            GetRoutesBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<RouteSortField> pagedListRequest);
        Task<GetRoutesPeriodReportResponseModel> GetRoutesPeriodReportAsync(GetRoutesPeriodReportRequestModel periodReportRequest);
        Task<GetRouteResponseModel> CreateAsync(CreateRouteRequestModel model);
        Task UpdateAsync(UpdateRouteRequestModel model);
        Task DeleteAsync(int id);
    }
}
