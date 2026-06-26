using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<ApiResponse<UserResponseDTO>> AuthenticateUserAsync(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
        }

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
            if ( await _userRepository.EmailExistsAsync(user.Email))
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
                Role = user.Role,
                Created = DateOnly.FromDateTime(DateTime.UtcNow),
                LastLogin = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true,
                ApiKey = Guid.NewGuid().ToString()
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

        public async Task<ApiResponse<object>> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if(user == null)
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

        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();

                if(users == null || !users.Any())
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
                    Message = "Succesfully retrieved list of users!",
                    Data = userResponses,
                    StatusCode=200
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

                if(users == null || !users.Any())
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

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserAsync(int userId, UpdateUserRequestDTO updatedUser)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);

                if(user == null)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                if(string.IsNullOrEmpty(updatedUser.UserName) || string.IsNullOrEmpty(updatedUser.Email) 
                    || string.IsNullOrEmpty(updatedUser.Password) || !updatedUser.Email.Contains("@") || !updatedUser.Email.Contains("."))
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "Make Sure all User Fields are correctly formatted.",
                        StatusCode = 400
                    };
                }
                if(await _userRepository.EmailExistsAsync(updatedUser.Email))
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "Email is already in use.",
                        StatusCode = 400
                    };
                }
                if(string.IsNullOrEmpty(updatedUser.Password) || updatedUser.Password.Length < 8 || !updatedUser.Password.Any(char.IsUpper) || !updatedUser.Password.Any(char.IsLower) 
                    || !updatedUser.Password.Any(char.IsDigit) || !updatedUser.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.",
                        StatusCode = 400
                    };
                }

                user.UserName = updatedUser.UserName;
                user.Email = updatedUser.Email;
                user.Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(updatedUser.Password);
                user.LastUpdated = DateTime.UtcNow;

                return new ApiResponse<UserResponseDTO>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = new UserResponseDTO
                    {
                        ID = user.ID,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        LastLogin = user.LastLogin,
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
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);

                if(user == null)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                user.Role = roleRequest.NewRole;
                user.LastUpdated = DateTime.UtcNow;

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
    }
}
