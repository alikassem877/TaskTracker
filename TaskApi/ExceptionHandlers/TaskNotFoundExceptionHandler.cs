
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Services;

namespace TaskApi.ExceptionHandlers;

public class TaskNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TaskNotFoundException)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Task Not Found",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken
        );

        return true;
    }
}
