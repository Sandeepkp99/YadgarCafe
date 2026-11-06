using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Common.Exceptions;

namespace YadgarCafe.API.Middlewares
{
    /// <summary>Turns exceptions into RFC 7807 problem responses so controllers need no try/catch.</summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Client went away; nothing to report.
                context.Response.StatusCode = 499;
            }
            catch (Exception ex)
            {
                await WriteProblemAsync(context, ex);
            }
        }

        private async Task WriteProblemAsync(HttpContext context, Exception ex)
        {
            var (status, title) = ex switch
            {
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Server error")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                return;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                // Internal details never reach the client.
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Please try again later."
                    : ex.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
