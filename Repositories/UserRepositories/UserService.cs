
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Repositories.UserRoleRepositories;
using InventoryManagementAPI.Repositories.UserRoles;
using Microsoft.EntityFrameworkCore;
using InventoryManagementAPI.Services;
using System.Net.Mail;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
        }

        // === GET ===
        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieves all users from the repository and validates the response to ensure that users were found.
                var users = await _userRepository.GetAllUsersAsync(cancellationToken);

                // Validates the retrieved users and builds a response based on whether users were found or not.
                return ValidateGetUserGroupBuildResponse(users);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<UserResponseDTO>>("Internal error occurred, failed to load all users.", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieves the user by email from the repository and checks if the user exists. If the user is not found, it returns a 404 Not Found response.
                var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);

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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to load user by email.", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var userResult = await ValidateUserIdAsync(userId, cancellationToken);
                if (userResult.User == null) return userResult.Error!;
                return BuildUserResponse(userResult.User, "Successfully retrieved user!", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to load user by ID.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {

            if (! await _roleRepository.CheckRoleExistAsync(roleName, cancellationToken))
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
                var users = await _userRepository.GetUsersByRoleAsync(roleName, cancellationToken);
                // Validates the retrieved users and builds a response based on whether users were found or not.
                return ValidateGetUserGroupBuildResponse(users);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<UserResponseDTO>>("Internal error occurred, failed to load users by role.", 500);
            }
        }

        // === POST ===
        public async Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user, CancellationToken cancellationToken = default)
        {
            //Validates the existence of the user DTO and checks for missing data. If the DTO is null, it returns a 404 Not Found response with an appropriate error message.
            var validateDtoExists = ValidateDtoExists(user, "User creation request data is missing.");
            if (validateDtoExists != null) return validateDtoExists;
            // Validates the fields of the CreateNewUserRequestDTO to ensure that all required fields are present and correctly formatted.
            // If any validation fails, it returns a 400 Bad Request response with an appropriate error message.
            var validateFields = await ValidateCreateNewUserDtoFields(user, cancellationToken);
            if (validateFields != null) return validateFields;

            // Create new user object and hash the password
            // Note: The password hashing is done using BCrypt with the EnhancedHashPassword method for improved security.
            var newUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password),
                Created = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                IsActive = true, 
            };

            try
            {
                //Save New User as Staff Role by default
                var staffRole = await _roleRepository.GetRoleByNameAsync("Staff", cancellationToken);
                if (staffRole == null || !staffRole.IsActive)
                {
                    return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to assign staff role.", 500);
                }

                newUser.UserRoles.Add(new UserRole
                {
                    RoleID = staffRole.ID,
                });

                // Attempts to create the new user in the repository and checks if the creation was successful.
                var createdUser = await _userRepository.CreateUserAsync(newUser, cancellationToken);

                //Saves the changes to the repository to persist the new user in the database.
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Builds a response for the newly created user using the BuildUserResponse method and returns it with a 201 Created status code.
                return BuildUserResponse(createdUser, "User created successfully", 201);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to create user.", 500);
            }
        }

        // === PATCH ===

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserEmailAsync(int userId, UpdateUserEmailRequestDTO emailRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default)
        {
            //Validates the existence of the email update request DTO and checks for missing data.
            //If the DTO is null, it returns a 404 Not Found response with an appropriate error message.
            var validateDtoExists = ValidateDtoExists(emailRequest, "Email update request data is missing.");
            if (validateDtoExists != null) return validateDtoExists;

            var rowVersionValidation = RowVersionHelper.ValidateFormat<UserResponseDTO>(emailRequest.RowVersion);
            if (rowVersionValidation != null) return rowVersionValidation;

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

            //Checks Jwt Claims to see if either Admin or corresponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if (authValidation != null) return authValidation;

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId, cancellationToken);
                if (result.User == null) return result.Error!;
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User;

                var matchingRowVersionValidation = RowVersionHelper.Validate<UserResponseDTO>(user.RowVersion, emailRequest.RowVersion);
                if (matchingRowVersionValidation != null) return matchingRowVersionValidation;

                if (!string.Equals(user.Email, emailRequest.Email, StringComparison.OrdinalIgnoreCase)
                    && await _userRepository.EmailExistsAsync(emailRequest.Email, cancellationToken))
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
                user.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Builds a response for the updated user using the BuildUserResponse method and returns it with a 200 OK status code.
                return BuildUserResponse(user, "Email updated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Concurrency error occurred, failed to update user. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to update user email.", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserNameAsync(int userId, UpdateUserNameRequestDTO nameRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default)
        {
            //Validates the existence of the username update request DTO and checks for missing data.
            var dtoValidation = ValidateDtoExists(nameRequest, "UserName update request data is missing.");
            if (dtoValidation != null) return dtoValidation;

            var rowVersionValidation = RowVersionHelper.ValidateFormat<UserResponseDTO>(nameRequest.RowVersion);
            if (rowVersionValidation != null) return rowVersionValidation;

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

            //Checks Jwt Claims to see if either Admin or corresponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if (authValidation != null) return authValidation;

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId, cancellationToken);
                if (result.Error != null) return result.Error;
                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                var matchingRowVersionValidation = RowVersionHelper.Validate<UserResponseDTO>(user.RowVersion, nameRequest.RowVersion);
                if (matchingRowVersionValidation != null) return matchingRowVersionValidation;

                // Saves the updated username to the user object and persists the changes to the repository.
                user.UserName = nameRequest.UserName;
                user.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return BuildUserResponse(user, "UserName updated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Concurrency error occurred, failed to update user. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to update user name.", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUserPasswordAsync(int userId, UpdateUserPasswordRequestDTO passwordRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default)
        {
            //Validates the existence of the password update request DTO and checks for missing data.
            var dtoValidation = ValidateDtoExists(passwordRequest, "Password update request data is missing.");
            if (dtoValidation != null) return dtoValidation;

            var rowVersionValidation = RowVersionHelper.ValidateFormat<UserResponseDTO>(passwordRequest.RowVersion);
            if (rowVersionValidation != null) return rowVersionValidation;

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

            //Checks Jwt Claims to see if either Admin or corresponding User
            var authValidation = ValidateAuthentication(currentUserRole, currentUserId, userId);
            if (authValidation != null) return authValidation;

            try
            {
                // Retrieves the user by ID and checks for errors in the response. If an error occurs (e.g., user not found), it returns the error response.
                var result = await ValidateUserIdAsync(userId, cancellationToken);
                if (result.Error != null) return result.Error;

                // Assigns the user from the result tuple to a variable for easier access
                var user = result.User!;

                var matchingRowVersionValidation = RowVersionHelper.Validate<UserResponseDTO>(user.RowVersion, passwordRequest.RowVersion);
                if (matchingRowVersionValidation != null) return matchingRowVersionValidation;

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
                user.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return BuildUserResponse(user, "Password updated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Concurrency error occurred, failed to update user. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to update user password.", 500);
            }
        }

 public async Task<ApiResponse<UserResponseDTO>> AssignUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResult = await ValidateUserIdAsync(userId, cancellationToken);
                if (userResult.Error is not null) return userResult.Error;
                var user = userResult.User!;

                var role = await _roleRepository.GetRoleAsync(roleId, cancellationToken);
                if (role is null)
                    return ApiResponseHelper.Failure<UserResponseDTO>("Role not found.", 404);

                if (!role.IsActive)
                    return ApiResponseHelper.Failure<UserResponseDTO>("Inactive roles cannot be assigned.", 409);

                if (await _userRoleRepository.UserRoleExistsAsync(userId, roleId, cancellationToken))
                    return BuildUserResponse(user, "User already has this role.", 200);

                await _userRoleRepository.AssignUserRoleAsync(userId, roleId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return BuildUserResponse(user, "User role assigned successfully.", 200);
            }
            catch (DbUpdateException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("The role assignment could not be completed because the membership changed concurrently.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to assign user role.", 500);
            }
        }

 public async Task<ApiResponse<UserResponseDTO>> RemoveUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResult = await ValidateUserIdAsync(userId, cancellationToken);
                if (userResult.Error is not null) return userResult.Error;
                var user = userResult.User!;

                var role = await _roleRepository.GetRoleAsync(roleId, cancellationToken);
                if (role is null)
                    return ApiResponseHelper.Failure<UserResponseDTO>("Role not found.", 404);

                var removed = await _userRoleRepository.RemoveUserRoleAsync(userId, roleId, cancellationToken);
                if (!removed)
                    return BuildUserResponse(user, "User does not have this role.", 200);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return BuildUserResponse(user, "User role removed successfully.", 200);
            }
            catch (DbUpdateException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("The role removal could not be completed because the membership changed concurrently.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to remove user role.", 500);
            }
        }

        // === SET ACTIVE / INACTIVE ===
        public async Task<ApiResponse<UserResponseDTO>> ActivateUserAsync(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default)
        {
            var dtoValidation = ValidateDtoExists(statusRequest, "User status update request data is missing.");
            if (dtoValidation != null) return dtoValidation;

            var rowVersionValidation = RowVersionHelper.ValidateFormat<UserResponseDTO>(statusRequest.RowVersion);
            if (rowVersionValidation != null) return rowVersionValidation;

            try
            {
                // Retrieve the user before attempting to activate them
                var userExists = await ValidateUserIdAsync(userId, cancellationToken);
                if (userExists.User == null) return userExists.Error!;

                var user = userExists.User;

                var matchingRowVersionValidation = RowVersionHelper.Validate<UserResponseDTO>(user.RowVersion, statusRequest.RowVersion);
                if (matchingRowVersionValidation != null) return matchingRowVersionValidation;

                // Return a bad request response if the user is already active
                if (user.IsActive || !statusRequest.IsActive)
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
                user.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return the activated user details
                return BuildUserResponse(user, "User activated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Concurrency error occurred, failed to update user. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to activate user.", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDTO>> DeactivateUserAsync(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default)
        {
            var dtoValidation = ValidateDtoExists(statusRequest, "User status update request data is missing.");
            if (dtoValidation != null) return dtoValidation;

            var rowVersionValidation = RowVersionHelper.ValidateFormat<UserResponseDTO>(statusRequest.RowVersion);
            if (rowVersionValidation != null) return rowVersionValidation;

            try
            {
                // Retrieve the user before attempting to deactivate them
                var userExists = await ValidateUserIdAsync(userId, cancellationToken);
                if (userExists.User == null) return userExists.Error!;
                var user = userExists.User;

                var matchingRowVersionValidation = RowVersionHelper.Validate<UserResponseDTO>(user.RowVersion, statusRequest.RowVersion);
                if (matchingRowVersionValidation != null) return matchingRowVersionValidation;

                // Return a bad request response if the user is already inactive
                if (!user.IsActive || statusRequest.IsActive)
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
                user.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return the deactivated user details
                return BuildUserResponse(user, "User deactivated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Concurrency error occurred, failed to update user. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<UserResponseDTO>("Internal error occurred, failed to deactivate user.", 500);
            }
        }



        // === HELPER METHODS ===

        // === Validation Methods ===
        private async Task<(User? User, ApiResponse<UserResponseDTO>? Error)> ValidateUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            // Retrieve the user and return a reusable not found response when missing
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
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
        private async Task<ApiResponse<UserResponseDTO>?> ValidateCreateNewUserDtoFields(CreateNewUserRequestDTO user, CancellationToken cancellationToken = default)
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
            if (await _userRepository.EmailExistsAsync(user.Email, cancellationToken))
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

        // === BUILDER METHODS ===
        private static UserResponseDTO MapToUserResponseDto(User user)
        {
            return new UserResponseDTO
            {
                ID = user.ID,
                UserName = user.UserName,
                Email = user.Email,
                Created = user.Created,
                LastLogin = user.LastLogin,
                Updated = user.Updated,
                IsActive = user.IsActive,
                RowVersion = user.RowVersion
            };
        }
        private ApiResponse<UserResponseDTO> BuildUserResponse(User user, string message, int statusCode)
        {
            return ApiResponseHelper.Success(MapToUserResponseDto(user), message, statusCode);
        }
    }
}
