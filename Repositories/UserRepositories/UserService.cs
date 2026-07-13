
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using System.Net.Mail;

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
                // Retrieves all users from the repository and validates the response to ensure that users were found.
                var users = await _userRepository.GetAllUsersAsync();

                // Validates the retrieved users and builds a response based on whether users were found or not.
                return ValidateGetUserGroupBuildResponse(users);
            }
            catch
            {
                return BuildBulkCatchErrorResponse("Internal error occurred, failed to load all users.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email)
        {
            try
            {
                // Retrieves the user by email from the repository and checks if the user exists. If the user is not found, it returns a 404 Not Found response.
                var user = await _userRepository.GetUserByEmailAsync(email);

                // Validates the retrieved user and builds a response based on whether the user was found or not.
                if (user == null)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                return BuildUserResponse(user, "Successfully retrieved user!", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to load user by email.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId)
        {
            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var userResult = await ValidateUserIdAsync(userId);
                if (userResult.User == null)
                {
                    //Returns the error response if the user is not found
                    return userResult.Error!;
                }
                return BuildUserResponse(userResult.User, "Successfully retrieved user!", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to load user by ID.");
            }
        }

        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(UserRoles role)
        {
            // Validates the provided role to ensure it is a valid UserRoles enum value. If the role is invalid, it returns a 400 Bad Request response.
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
                // Retrieves users by role from the repository and validates the response to ensure that users were found.
                var users = await _userRepository.GetUsersByRoleAsync(role);
                // Validates the retrieved users and builds a response based on whether users were found or not.
                return ValidateGetUserGroupBuildResponse(users);
            }
            catch
            {
                return BuildBulkCatchErrorResponse("Internal error occurred, failed to load users by role.");
            }
        }

        // === POST === \\
        public async Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user)
        {
            //Validates the existence of the user DTO and checks for missing data. If the DTO is null, it returns a 404 Not Found response with an appropriate error message.
            var validateDtoExists = ValidateDtoExists(user, "User creation request data is missing.");
            if(validateDtoExists != null)
            {
                return validateDtoExists;
            }
            // Validates the fields of the CreateNewUserRequestDTO to ensure that all required fields are present and correctly formatted.
            // If any validation fails, it returns a 400 Bad Request response with an appropriate error message.
            var validateFields = await ValidateCreateNewUserDtoFields(user);
            if (validateFields != null)
            {
                return validateFields;
            }

            // Create new user object and hash the password
            // Note: The password hashing is done using BCrypt with the EnhancedHashPassword method for improved security.
            var newUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password),
                Role = UserRoles.Staff,
                Created = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true,
            };

            try
            {
                // Attempts to create the new user in the repository and checks if the creation was successful.
                var createdUser = await _userRepository.CreateUserAsync(newUser);

                if(createdUser == null)
                {
                    // If the creation fails (e.g., due to a database error), it returns a 500 Internal Server Error response.
                    return BuildSingleCatchErrorResponse("Internal error occurred, failed to create user.");
                }

                // Builds a response for the newly created user using the BuildUserResponse method and returns it with a 201 Created status code.
                return BuildUserResponse(createdUser, "User created successfully", 201);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to create user.");
            }
        }

        // === PATCH === \\

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserEmailAsync(int userId, UpdateUserEmailRequestDTO emailRequest, int currentUserId, string currentUserRole)
        {
            //Validates the existence of the email update request DTO and checks for missing data.
            //If the DTO is null, it returns a 404 Not Found response with an appropriate error message.
            var validateDtoExists = ValidateDtoExists(emailRequest, "Email update request data is missing.");
            if (validateDtoExists != null)
            {
                return validateDtoExists;
            }

            // Validates the new email format using a simple check for the presence of "@" and "." characters.
            // If the email is invalid, it returns a 400 Bad Request response.
            if (!IsValidEmail(emailRequest.Email))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Email format",
                    StatusCode = 400
                };
            }

            //Checks Jwt Claims to see if either Admin or Corrosponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if(authValidation != null)
            {
                // If the user is not authorized to update the email (i.e., they are neither an admin nor the owner of the account), it returns a 403 Forbidden response.
                return authValidation;
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId);
                if (result.User == null)
                {
                    return result.Error!;
                }
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User;

                if (!string.Equals(user.Email, emailRequest.Email, StringComparison.OrdinalIgnoreCase)
                    && await _userRepository.EmailExistsAsync(emailRequest.Email))
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "Email is already in use.",
                        StatusCode = 400
                    };
                }

                // Saves the updated email to the user object and persists the changes to the repository.
                user.Email = emailRequest.Email;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                // Builds a response for the updated user using the BuildUserResponse method and returns it with a 200 OK status code.
                return BuildUserResponse(user, "Email updated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to update user email.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserNameAsync(int userId, UpdateUserNameRequestDTO nameRequest, int currentUserId, string currentUserRole)
        {
            //Validates the existence of the username update request DTO and checks for missing data.
            var dtoValidation = ValidateDtoExists(nameRequest, "UserName update request data is missing.");
            if (dtoValidation != null)
            {
                return dtoValidation;
            }

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

            //Checks Jwt Claims to see if either Admin or Corrosponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if(authValidation != null)
            {
                // If the user is not authorized to update the username (i.e., they are neither an admin nor the owner of the account), it returns a 403 Forbidden response.
                return authValidation;
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId);
                if (result.Error != null)
                {
                    return result.Error;
                }
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                // Saves the updated username to the user object and persists the changes to the repository.
                user.UserName = nameRequest.UserName;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                return BuildUserResponse(user, "UserName updated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to update user name.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserPasswordAsync(int userId, UpdateUserPasswordRequestDTO passwordRequest, int currentUserId, string currentUserRole)
        {
            //Validates the existence of the password update request DTO and checks for missing data.
            var dtoValidation = ValidateDtoExists(passwordRequest, "Password update request data is missing.");
            if (dtoValidation != null)
            {
                return dtoValidation;
            }

            // Validates the new password for complexity requirements (length, uppercase, lowercase, digit, special character).
            if (string.IsNullOrEmpty(passwordRequest.NewPassword) || passwordRequest.NewPassword.Length < 8 || !passwordRequest.NewPassword.Any(char.IsUpper) 
                || !passwordRequest.NewPassword.Any(char.IsLower) || !passwordRequest.NewPassword.Any(char.IsDigit) 
                || !passwordRequest.NewPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "New password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.",
                    StatusCode = 400
                };
            }

            // Validates that the new password and retype password match. If they do not match, it returns a 400 Bad Request response.
            if (passwordRequest.NewPassword != passwordRequest.RetypePassword)
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "New password and retype password do not match.",
                    StatusCode = 400
                };
            }

            //Checks Jwt Claims to see if either Admin or Corrosponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if (authValidation != null)
            {
                // If the user is not authorized to update the username (i.e., they are neither an admin nor the owner of the account), it returns a 403 Forbidden response.
                return authValidation;
            }

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId);
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
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                return BuildUserResponse(user, "Password updated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to update user password.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserRoleAsync(int userId, UpdateUserRoleRequestDTO roleRequest)
        {
            //Validates the existence of the role update request DTO and checks for missing data.
            var validateDto = ValidateDtoExists(roleRequest, "Role update request data is missing.");
            if(validateDto != null)
            {
                return validateDto;
            }

            // Validates the new role provided in the request to ensure it is a valid UserRoles enum value. If the role is invalid, it returns a 400 Bad Request response.
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
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var userResult = await ValidateUserIdAsync(userId);
                if (userResult.Error != null)
                {
                    return userResult.Error;
                }

                // Assigns the user from the result tuple to a variable for easier access
                var user = userResult.User!;

                // Updates the user's role to the new role provided in the request and saves the changes to the repository.
                user.Role = roleRequest.NewRole;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                // Builds a response for the updated user using the BuildUserResponse method and returns it with a 200 OK status code.
                return BuildUserResponse(user, "User role updated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to update user role.");
            }
        }

        // === SET ACTIVE / INACTIVE === \\
        public async Task<ApiResponse<UserResponseDTO>> ActivateUserAsync(int userId)
        {
            try
            {
                // Retrieve the user before attempting to activate them
                var userExists = await ValidateUserIdAsync(userId);
                if(userExists.User == null)
                {
                    return userExists.Error!;
                }

                var user = userExists.User;

                // Return a bad request response if the user is already active
                if (user.IsActive)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User is already active",
                        StatusCode = 400
                    };
                }

                // Set the user active and update the timestamp
                user.IsActive = true;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                // Return the activated user details
                return BuildUserResponse(user, "User activated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to activate user.");
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> DeactivateUserAsync(int userId)
        {
            try
            {
                // Retrieve the user before attempting to deactivate them
                var userExists = await ValidateUserIdAsync(userId);
                if(userExists.User == null)
                {
                    return userExists.Error!;
                }
                var user = userExists.User;

                // Return a bad request response if the user is already inactive
                if (!user.IsActive)
                {
                    return new ApiResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = "User is already inactive",
                        StatusCode = 400
                    };
                }

                // Set the user inactive and update the timestamp
                user.IsActive = false;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                // Return the deactivated user details
                return BuildUserResponse(user, "User deactivated successfully", 200);
            }
            catch
            {
                return BuildSingleCatchErrorResponse("Internal error occurred, failed to deactivate user.");
            }
        }



        // === HELPER METHODS === \\

        // === Validation Methods === \\
        private async Task<(User? User, ApiResponse<UserResponseDTO>? Error)> ValidateUserIdAsync(int userId)
        {
            // Retrieve the user and return a reusable not found response when missing
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
        private ApiResponse<IEnumerable<UserResponseDTO>> ValidateGetUserGroupBuildResponse(IEnumerable<User> users)
        {
            if (users == null || !users.Any())
            {
                return new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = "No User's found",
                    StatusCode = 404
                };
            }
            var userResponses = users.Select(MapToUserResponseDto).ToList();

            return new ApiResponse<IEnumerable<UserResponseDTO>>
            {
                Success = true,
                Message = "Successfully retrieved list of users!",
                Data = userResponses,
                StatusCode = 200
            };
        }
        private ApiResponse<UserResponseDTO>? ValidateDtoExists(object? obj, string errorMessage)
        {
            if (obj == null)
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = 400
                };
            }
            return null;
        }
        private async Task<ApiResponse<UserResponseDTO>?> ValidateCreateNewUserDtoFields(CreateNewUserRequestDTO user)
        {
            // Basic validation for user input
            // This validation checks if the UserName, Email, and Password fields are not empty, and if the email is in a valid format
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email)
                || string.IsNullOrEmpty(user.Password) || !IsValidEmail(user.Email))
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

            // Validate that the password and retype password match. If they do not match, it returns a 400 Bad Request response.
            if (user.Password != user.RetypePassword)
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Password and Retype Password do not match.",
                    StatusCode = 400
                };
            }
            return null;
        }
        private ApiResponse<UserResponseDTO>? ValidateAuthentication(string currentUserRole, int currentUserId, int userId)
        {
            if (currentUserRole != "Admin" && currentUserId != userId)
            {
                return new ApiResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Unauthorized! Can only update your own account.",
                    StatusCode = 403
                };
            }
            return null;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // === BUILDER METHODS === \\
        private static UserResponseDTO MapToUserResponseDto(User user)
        {
            return new UserResponseDTO
            {
                ID = user.ID,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                Created = user.Created,
                LastLogin = user.LastLogin,
                LastUpdated = user.LastUpdated,
                IsActive = user.IsActive
            };
        }

        private ApiResponse<UserResponseDTO> BuildUserResponse(User user, string message, int statusCode)
        {
            return new ApiResponse<UserResponseDTO>
            {
                Success = true,
                Message = message,
                Data = MapToUserResponseDto(user),
                StatusCode = statusCode
            }; 
        }
        private ApiResponse<IEnumerable<UserResponseDTO>> BuildBulkCatchErrorResponse(string message)
        {
            return new ApiResponse<IEnumerable<UserResponseDTO>>
            {
                Success = false,
                Message = message,
                StatusCode = 500,
            };
        }
        private ApiResponse<UserResponseDTO> BuildSingleCatchErrorResponse(string message)
        {
            return new ApiResponse<UserResponseDTO>
            {
                Success = false,
                Message = message,
                StatusCode = 500,
            };
        }
    }
}
