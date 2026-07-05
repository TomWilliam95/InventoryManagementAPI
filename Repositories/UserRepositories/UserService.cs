
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // === GET === \\
        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();

                if (users == null || !users.Any())
                {
                    return new ApiResponse<IEnumerable<UserResponseDTO>>
                    {
                        Success = false,
                        Message = "No User's found",
                        StatusCode = 404
                    };
                }
                var userResponses = users.Select(user => new UserResponseDTO
                {
                    ID = user.ID,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    Created = user.Created,
                    LastLogin = user.LastLogin,
                    LastUpdated = user.LastUpdated,
                    IsActive = user.IsActive,
                }).ToList();

                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully retrieved list of users!",
                    Data = userResponses,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                var userResponse = new UserResponseDTO
                {
                    ID = user.ID,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    LastLogin = user.LastLogin,
                    Created = user.Created,
                    IsActive = user.IsActive
                };
                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = userResponse,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var userResponse = new UserResponseDTO
                {
                    ID = user.ID,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    LastLogin = user.LastLogin,
                    Created = user.Created,
                    IsActive = user.IsActive
                };
                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = userResponse,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(UserRoles role)
        {
            if (!Enum.IsDefined(typeof(UserRoles), role))
            {
                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = "Invalid User Role",
                    StatusCode = 400
                };
            }
            try
            {
                var users = await _userRepository.GetUsersByRoleAsync(role);

                if (users == null || !users.Any())
                {
                    return new ApiResponse<IEnumerable<UserResponseDTO>>
                    {
                        Success = false,
                        Message = "No users found with the specified role",
                        StatusCode = 404
                    };
                }

                var userResponses = users.Select(user => new UserResponseDTO
                {
                    ID = user.ID,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    LastLogin = user.LastLogin,
                    Created = user.Created,
                    IsActive = user.IsActive
                }).ToList();

                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = userResponses,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        // === POST === \\
        public async Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user)
        {
            // Basic validation for user input
            // This validation checks if the UserName, Email, and Password fields are not empty, and if the email is in a valid format
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email)
                || string.IsNullOrEmpty(user.Password) || !user.Email.Contains("@") || !user.Email.Contains("."))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Make Sure all User Fields are correctly formatted.",
                    StatusCode = 400
                };
            }

            // Check if email is already in use
            // This check ensures that the email provided for the new user is not already associated with an existing user in the system.
            if (await _userRepository.EmailExistsAsync(user.Email))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Email is already in use.",
                    StatusCode = 400
                };
            }

            // Validate password complexity
            // This validation ensures that the password meets certain complexity requirements, such as being at least 8 characters long and
            // containing uppercase letters, lowercase letters, digits, and special characters.
            if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 8 || !user.Password.Any(char.IsUpper) || !user.Password.Any(char.IsLower)
                || !user.Password.Any(char.IsDigit) || !user.Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.",
                    StatusCode = 400
                };
            }

            // Create new user object and hash the password
            // Note: The password hashing is done using BCrypt with the EnhancedHashPassword method for improved security.
            // The new user is initialized with the provided details, and additional fields such as Created,
            // LastLogin, LastUpdated, IsActive, and ApiKey are set accordingly.
            var newUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password),
                Role = UserRoles.Staff,
                Created = DateOnly.FromDateTime(DateTime.UtcNow),
                LastLogin = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true,
            };

            try
            {
                await _userRepository.CreateUserAsync(newUser);

                var createdUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = new UserResponseDTO
                    {
                        ID = createdUser.ID,
                        UserName = createdUser.UserName,
                        Email = createdUser.Email,
                        Role = createdUser.Role,
                        Created = createdUser.Created,
                        LastLogin = createdUser.LastLogin,
                        LastUpdated = createdUser.LastUpdated,
                        IsActive = createdUser.IsActive
                    },
                    StatusCode = 201
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        // === PATCH === \\

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserEmailAsync(int userId, UpdateUserEmailRequestDTO emailRequest)
        {
            // Validates the new email format using a simple check for the presence of "@" and "." characters.
            // If the email is invalid, it returns a 400 Bad Request response.
            if (string.IsNullOrEmpty(emailRequest.Email) || !emailRequest.Email.Contains("@") || !emailRequest.Email.Contains("."))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Email format",
                    StatusCode = 400
                };
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await GetUserByIdWithResponseAsync(userId);
                if(result.Error != null)
                {
                    return result.Error;
                }
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                // Saves the updated email to the user object and persists the changes to the repository.
                user.Email = emailRequest.Email;
                await _userRepository.SaveChangesAsync();

                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "Email updated successfully",
                    Data = new UserResponseDTO
                    {
                        ID = user.ID,
                        UserName = user.UserName,
                        Email = emailRequest.Email, // Updated email
                        Role = user.Role,
                        LastLogin = user.LastLogin,
                        LastUpdated = DateTime.UtcNow, 
                        Created = user.Created,
                        IsActive = user.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserNameAsync(int userId, UpdateUserNameRequestDTO nameRequest)
        {
            // Validates the new username for length and character requirements (alphanumeric, no spaces).
            if (string.IsNullOrEmpty(nameRequest.UserName) || nameRequest.UserName.Length < 3 || nameRequest.UserName.Length > 50
                || !nameRequest.UserName.All(char.IsLetterOrDigit) || nameRequest.UserName.Contains(" "))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Invalid UserName",
                    StatusCode = 400
                };
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await GetUserByIdWithResponseAsync(userId);
                if(result.Error != null)
                {
                    return result.Error;
                }
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                // Saves the updated username to the user object and persists the changes to the repository.
                user.UserName = nameRequest.UserName;
                await _userRepository.SaveChangesAsync();

                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "UserName updated successfully",
                    Data = new UserResponseDTO
                    {
                        ID = user.ID,
                        UserName = nameRequest.UserName, // Updated username
                        Email = user.Email,
                        Role = user.Role,
                        LastLogin = user.LastLogin,
                        LastUpdated = DateTime.UtcNow, 
                        Created = user.Created,
                        IsActive = user.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserPasswordAsync(int userId, UpdateUserPasswordRequestDTO passwordRequest)
        {

            // Validates the new password for complexity requirements (length, uppercase, lowercase, digit, special character).
            if (string.IsNullOrEmpty(passwordRequest.NewPassword) || passwordRequest.NewPassword.Length < 8 || !passwordRequest.NewPassword.Any(char.IsUpper) || !passwordRequest.NewPassword.Any(char.IsLower)
                || !passwordRequest.NewPassword.Any(char.IsDigit) || !passwordRequest.NewPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "New password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.",
                    StatusCode = 400
                };
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await GetUserByIdWithResponseAsync(userId);
                if (result.Error != null)
                {
                    return result.Error;
                }

                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                // Validates the current password provided by the user against the stored password hash using BCrypt's EnhancedVerify method.
                if (!BCrypt.Net.BCrypt.EnhancedVerify(passwordRequest.CurrentPassword, user.Password_Hash))
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        StatusCode = 400
                    };
                } 

                user.Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(passwordRequest.NewPassword);
                await _userRepository.SaveChangesAsync();

                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "Password updated successfully",
                    Data = new UserResponseDTO
                    {
                        ID = user.ID,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        LastLogin = user.LastLogin,
                        LastUpdated = user.LastUpdated,
                        Created = user.Created,
                        IsActive = user.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };  
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserRoleAsync(int userId, UpdateUserRoleRequestDTO roleRequest)
        {
            if (!Enum.IsDefined(typeof(UserRoles), roleRequest.NewRole))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Invalid User Role",
                    StatusCode = 400
                };
            }
            try
            {
                var userResult = await GetUserByIdWithResponseAsync(userId);
                if (userResult.Error != null)
                {
                    return userResult.Error;
                }

                // Assigns the user from the result tuple to a variable for easier access
                var user = userResult.User!;

                // Updates the user's role to the new role provided in the request and saves the changes to the repository.
                user.Role = roleRequest.NewRole;
                await _userRepository.SaveChangesAsync();

                
                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "User role updated successfully",
                    Data = new UserResponseDTO
                    {
                        ID = user.ID,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        LastLogin = user.LastLogin,
                        LastUpdated = user.LastUpdated,
                        Created = user.Created,
                        IsActive = user.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        // === DELETE === \\
        public async Task<ApiResponse<object>> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not Found",
                        StatusCode = 404
                    };
                }

                await _userRepository.DeleteUserAsync(userId);
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User was Deleted",
                    StatusCode = 204
                };
            }
            catch
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        // === AUTHENTICATION === \\
        public Task<ApiResponse<UserResponseDTO>> AuthenticateUserAsync(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
        }

        // === HELPER METHODS === \\
        private async Task<(User? User, ApiResponse<UserResponseDTO>? Error)> GetUserByIdWithResponseAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return (null, new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = 404
                });
            }
            return (user, null);
        }
    }
}
