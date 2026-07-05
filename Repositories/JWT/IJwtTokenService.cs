using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.JWT
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
