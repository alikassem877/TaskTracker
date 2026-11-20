using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskApi.DTOs;
using TaskApi.ExceptionHandlers;
using TaskTracker.Data;
using TaskTracker.Repositories;
using TaskTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI and validation
builder.Services.AddOpenApi();
builder.Services.AddValidation();

// Exception handling
builder.Services.AddExceptionHandler<TaskNotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));

    // Log SQL queries only in Development
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Dependency injection
builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

// HTTP pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// GET: Retrieve all tasks
app.MapGet("/api/tasks", async (
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var tasks = await service.GetAllTasksAsync(cancellationToken);
    return Results.Ok(tasks);
});

// GET: Retrieve a task by ID
app.MapGet("/api/tasks/{id}", async (
    int id,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var task = await service.GetTaskByIdAsync(id, cancellationToken);
    return Results.Ok(task);
});

// POST: Create a task
app.MapPost("/api/tasks", async (
    CreateTaskRequest request,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var task = await service.AddTaskAsync(
        request.Title,
        request.Priority,
        cancellationToken);

    return Results.Created($"/api/tasks/{task.Id}", task);
});

// PUT: Complete a task
app.MapPut("/api/tasks/{id}/complete", async (
    int id,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    await service.CompleteAsync(id, cancellationToken);
    return Results.NoContent();
});

// DELETE: Delete a task
app.MapDelete("/api/tasks/{id}", async (
    int id,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    await service.DeleteTaskAsync(id, cancellationToken);
    return Results.NoContent();
});

app.Run();
