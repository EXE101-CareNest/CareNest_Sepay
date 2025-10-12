using System.Net;
using System.Text.Json;
using CareNest_SePay.Application.Exceptions;
using CareNest_SePay.Application.Common;

namespace CareNest_SePay.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = exception switch
            {
                ValidationException validationEx => new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationEx.ValidationResults.Select(vr => vr.ErrorMessage ?? "").ToList()
                },
                BusinessException businessEx => new BaseResponse<object>
                {
                    Success = false,
                    Message = businessEx.Message,
                    Errors = new List<string> { businessEx.ErrorCode }
                },
                NotFoundException notFoundEx => new BaseResponse<object>
                {
                    Success = false,
                    Message = notFoundEx.Message,
                    Errors = new List<string> { "NOT_FOUND" }
                },
                UnauthorizedAccessException => new BaseResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized access",
                    Errors = new List<string> { "UNAUTHORIZED" }
                },
                _ => new BaseResponse<object>
                {
                    Success = false,
                    Message = "An internal server error occurred",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                }
            };

            response.StatusCode = exception switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                BusinessException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }
}
