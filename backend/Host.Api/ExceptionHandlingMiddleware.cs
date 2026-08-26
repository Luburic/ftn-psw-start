using Shared.Domain.Exceptions;

namespace Host.Api;

public sealed class ExceptionHandlingMiddleware
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
        catch (NotFoundException exception)
        {
            await Results.Problem(statusCode: StatusCodes.Status404NotFound, title: exception.Message)
                .ExecuteAsync(context);
        }
        catch (DomainException exception)
        {
            await Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: exception.Message)
                .ExecuteAsync(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.")
                .ExecuteAsync(context);
        }
    }
}
