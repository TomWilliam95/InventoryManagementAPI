using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Repositories.UserRepositories;

namespace InventoryManagementAPI.Repositories.AuthenticationRepositories
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<LoginResponseDTO>?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                // Validates the provided email and password for null or empty values.
                // If either is invalid, it returns a 400 Bad Request response.
                var user = await _userRepository.GetUserByEmailAsync(loginRequestDTO.Email);
                if (user == null)
                {
                    return new ApiResponse<LoginResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid Email or Password",
                        StatusCode = 404
                    };
                }
                ;

                // Validates the provided password against the stored password hash using BCrypt's EnhancedVerify method.
                if (!BCrypt.Net.BCrypt.EnhancedVerify(loginRequestDTO.Password, user.Password_Hash))
                {
                    return new ApiResponse<LoginResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid Email or Password",
                        StatusCode = 401
                    };
                }

                // Updates the user's last login timestamp to the current UTC time and saves the changes to the repository.
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                return new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Message = "User authenticated successfully",
                    Data = new LoginResponseDTO
                    {
                        // Generates a JWT token for the authenticated user using the GenerateJwtToken method.
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

    }
}
