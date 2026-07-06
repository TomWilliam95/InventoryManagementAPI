using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Repositories.JWT;
using InventoryManagementAPI.Repositories.UserRepositories;

namespace InventoryManagementAPI.Repositories.AuthenticationRepositories
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenRepository;
        public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenRepository)
        {
            _userRepository = userRepository;
            _jwtTokenRepository = jwtTokenRepository;
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            // Validates that all fields have data
            if(loginRequestDTO == null || string.IsNullOrEmpty(loginRequestDTO.Email) || string.IsNullOrEmpty(loginRequestDTO.Password))
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Email and Password are required",
                    StatusCode = 400
                };
            }

            // Validates the provided email format to ensure it contains both "@" and "." characters.
            // If the email format is invalid, it returns a 400 Bad Request response.
            if (!loginRequestDTO.Email.Contains("@") || !loginRequestDTO.Email.Contains("."))
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Email format",
                    StatusCode = 400
                };
            }            
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
                        StatusCode = 401
                    };
                }

                if (!user.IsActive)
                {
                    return new ApiResponse<LoginResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid Email or Password",
                        StatusCode = 401
                    };
                }

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
                        Token = _jwtTokenRepository.GenerateToken(user)
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
