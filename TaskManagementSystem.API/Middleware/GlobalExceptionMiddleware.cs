using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using TaskManagementSystem.Core.Exceptions;

namespace TaskManagementSystem.API.Middleware
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
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<GlobalExceptionMiddleware> logger)
        {
            var (status, title) = exception switch
            {
                KeyNotFoundException    => (HttpStatusCode.NotFound,           "Resource Not Found"),
                ConflictException       => (HttpStatusCode.Conflict,           "Conflict"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,   "Unauthorized"),
                ArgumentException       => (HttpStatusCode.BadRequest,         "Bad Request"),
                InvalidOperationException => (HttpStatusCode.BadRequest,       "Bad Request"),
                _                       => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            var logLevel = status >= HttpStatusCode.InternalServerError ? LogLevel.Error : LogLevel.Warning;
            
            logger.Log(
                logLevel,
                exception,
                "Request failed with Status: {StatusCode} ({StatusTitle}) | TraceId: {TraceId} | Method: {Method} | Path: {Path} | Query: {QueryString} | User: {User} | IP: {IP}",
                (int)status,
                title,
                context.TraceIdentifier,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString.ToString(),
                context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonymous",
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            var problem = new ProblemDetails
            {
                Status   = (int)status,
                Title    = title,
                Detail   = exception.Message,
                Instance = context.Request.Path
            };
            
            // Add TraceIdentifier to the response so the client can report it
            problem.Extensions.Add("traceId", context.TraceIdentifier);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode  = (int)status;

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
