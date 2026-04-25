using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace CorePulse.Server.Infrastructure.Middlewares
{
    /// <summary>
    /// Global exception handling middleware.
    /// Intercepts all exceptions thrown during the HTTP request lifecycle to provide
    /// consistent, secure, and structured JSON responses to the client.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to process the current HTTP context.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request to the next middleware in the pipeline
                await _next(context);
            }
            catch (InvalidOperationException ex)
            {
                // Handles application-level business logic violations (e.g., trying to kill a non-existent process)
                _logger.LogWarning(ex, "Handled InvalidOperationException: Potential business logic violation.");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                // Handles Entity Framework Core database update errors (e.g., unique constraint violations)
                _logger.LogError(ex, "Database update exception occurred.");
                // We return a generic message for security to avoid leaking database schema details
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "A database error occurred.");
            }
            catch (Exception ex)
            {
                // Catch-all for any other unhandled exceptions
                _logger.LogError(ex, "An unhandled exception occurred in the request pipeline.");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Helper method to format and write the JSON error response to the client.
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode code, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new { message = message };
            var jsonResponse = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}