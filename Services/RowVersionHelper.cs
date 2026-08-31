using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Services
{
    public static class RowVersionHelper
    {
        public static ApiResponse<T>? ValidateFormat<T>(byte[]? supplied) =>
            supplied is null || supplied.Length == 0
            ? ApiResponseHelper.Failure<T>("Row version is required.", StatusCodes.Status400BadRequest)
            : supplied.Length != 8
            ? ApiResponseHelper.Failure<T>("Row version must be 8 bytes long.", StatusCodes.Status400BadRequest)
            : null;

        public static ApiResponse<T>? Validate<T>(byte[]? current, byte[]? supplied) =>
            ValidateFormat<T>(supplied) is { } formatError
            ? formatError
            : current is null || current.Length != 8
            ? ApiResponseHelper.Failure<T>("The stored row version is invalid.", StatusCodes.Status500InternalServerError)
            : (!current.SequenceEqual(supplied!))
            ? ApiResponseHelper.Failure<T>("Row version mismatch. The record has been modified by another user.", StatusCodes.Status409Conflict)
            : null;
    }
}
