using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAccountService
    {
        Task<ResponseAuth> LoginAsync(LoginModel model);
        Task<UserDTO> RegisterAsync(RegisterModel model);
        Task<IEnumerable<UserDTO>> GetDispatchersAsync();
        Task<UserDTO> GetDispatcherByIdAsync(string id);
        Task DeleteUserAsync(string id);
    }
}
