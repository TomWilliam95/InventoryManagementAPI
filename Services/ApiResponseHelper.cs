using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Services
{
    public static class ApiResponseHelper
    {
        public static ApiResponse<T> Success<T>(T data, string message, int statusCode = 200) => new() {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };

        public static ApiResponse<T> Failure<T>(string message, int statusCode) => new() {
            Success = false,
            Data = default,
            Message = message,
            StatusCode = statusCode
        };
    }
}
