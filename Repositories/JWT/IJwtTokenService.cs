using InventoryManagementAPI.Models.CoreModels.UserModels;

namespace InventoryManagementAPI.Repositories.JWT
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
