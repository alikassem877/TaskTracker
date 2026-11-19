using TaskTracker.Repositories;
using TaskTracker.Services;
using TaskApi.DTOs;
using TaskApi.ExceptionHandlers;
using TaskTracker.Models;
using Microsoft.AspNetCore.Http.HttpResults;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddExceptionHandler<TaskNotFoundExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<TaskIdGenerator>();
builder.Services.AddScoped<TaskService>();

builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();

app.MapGet("/api/tasks", async (TaskService service) =>
{
    var tasks = await service.GetAllTasksAsync();
    return Results.Ok(tasks);
});

app.MapGet("/api/tasks/{id}", async (int id, TaskService service) =>
{
    var task = await service.GetTaskByIdAsync(id);
    return Results.Ok(task);
});

app.MapPost("/api/tasks", async (CreateTaskRequest request, TaskService service) =>
{
    var task = await service.AddTaskAsync(request.Title, request.Priority);

    return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapPut("/api/tasks/{id}/complete", async (int id, TaskService service) =>
{
    await service.CompleteAsync(id);
    return Results.NoContent();
});

app.MapDelete("/api/tasks/{id}", async (int id, TaskService service) =>
{
    await service.DeleteTaskAsync(id);
    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
