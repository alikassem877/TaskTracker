using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Services;

namespace TaskApi.ExceptionHandlers;

public class TaskNotFoundExceptionHandler(
    ILogger<TaskNotFoundExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode;
        string title;

        if (exception is TaskNotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = "Task Not Found";
            logger.LogWarning(exception, "Task not found.");
        }
        else if (exception is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Invalid Request";
            logger.LogWarning(exception, "Invalid request.");
        }
        else
        {
            logger.LogError(exception, "An unexpected error occurred.");
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken
        );

        return true;
    }
}