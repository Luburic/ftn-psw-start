using Shared.Domain.Exceptions;

namespace Host.Api;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
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
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.")
                .ExecuteAsync(context);
        }
    }
}
