using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;

namespace InventoryManagementAPI.Repositories.AuthenticationRepositories
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest, CancellationToken cancellationToken = default);
    }
}
