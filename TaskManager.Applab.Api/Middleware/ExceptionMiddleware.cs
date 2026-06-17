using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Applab.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var response = ex switch
            {
                //404 - not found 
                KeyNotFoundException => new ErrorResponse(404, "Not Found", ex.Message),

                //400 - bad request / validation
                ArgumentException => new ErrorResponse(400, "Bad Request", ex.Message),

                //400 - invalid input format
                FormatException => new ErrorResponse(400, "Invalid Format", ex.Message),

                //401 - unathorized
                UnauthorizedAccessException => new ErrorResponse(401, "Unauthorized", "You are not authorized"),

                //403 - forbidden
                AccessViolationException => new ErrorResponse(403, "Forbidden", "You don't have permission"),

                //409 - conflict (dublicate email, etc)
                InvalidOperationException => new ErrorResponse(409, "conflict", ex.Message),

                //408 - timeout
                TimeoutException => new ErrorResponse(408, "Timeout", "Request timeout"),

                //503 - database connection error
                DbUpdateException => new ErrorResponse(503, "Database error", "An error occurred while saving data"),

                //404 - no result in sequence
                InvalidDataException => new ErrorResponse(404, "Invaild Data", ex.Message),
                
                //500 - everything else
                _ => new ErrorResponse(500, "Server error", "An unexpeted error occured")
            };

            context.Response.StatusCode = response.StatusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);

        }


    }

    public record ErrorResponse(int StatusCode, string Error, string Message);
}
