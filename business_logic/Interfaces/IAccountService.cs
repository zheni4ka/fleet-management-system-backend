using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAccountService
    {
        Task<LoginResponseModel> LoginAsync(LoginRequestModel model);
        Task<RefreshSessionResponseModel> RefreshSessionAsync(RefreshSessionRequestModel model);
        Task LogoutAsync(LogoutRequestModel model);
        Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel model);
        Task<ResetUserPasswordResponseModel> ResetUserPasswordAsync(ResetUserPasswordRequestModel model);
        Task<GetDispatcherResponseModel> RegisterAsync(RegisterAccountRequestModel model);
        Task<GetPagedListResponseModel<GetDispatcherResponseModel>> GetDispatchersPagedListAsync(GetPagedListRequestModel<DispatcherSortField> pagedListRequest);
        Task<GetDispatcherResponseModel> GetDispatcherByIdAsync(string id);
        Task UpdateUserDepotsAsync(UpdateUserDepotsRequestModel model);
        Task DeactivateUserAsync(string id);
        Task ReactivateUserAsync(string id);
        Task UpdatePreferredLanguageAsync(UpdatePreferredLanguageRequestModel model);
    }
}
