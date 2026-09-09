using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Services
{
    public static class ApiResponseFactory
    {
        public static ApiResponse<T> Success<T>(T data, string message, int statusCode = 200) => new()
        {
            Data = data,
            Success = true,
            Message = message,
            StatusCode = statusCode
        };

        public static ApiResponse<T> Error<T>(string message, int statusCode) => new()
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }
}
