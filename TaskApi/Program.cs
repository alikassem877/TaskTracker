
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TaskApi.DTOs;
using TaskApi.ExceptionHandlers;
using TaskTracker.Data;
using TaskTracker.Repositories;
using TaskTracker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

// OpenAPI and validation
builder.Services.AddOpenApi();
builder.Services.AddValidation();

// Exception handling
builder.Services.AddExceptionHandler<TaskNotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// Database configuration
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

var keyBytes = Convert.FromBase64String(jwtKey);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT key must be at least 256 bits.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Dependency injection
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

// Get authenticated user's ID from JWT
static int GetUserId(ClaimsPrincipal user)
{
    var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
    {
        throw new UnauthorizedAccessException("Invalid user identity.");
    }

    return userId;
}

// HTTP pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// GET: Retrieve all tasks belonging to the authenticated user
app.MapGet("/api/tasks", async (
    ClaimsPrincipal user,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);

    var tasks = await service.GetAllTasksAsync(
        userId,
        cancellationToken);

    return Results.Ok(tasks);
}).RequireAuthorization();

// GET: Retrieve one task belonging to the authenticated user
app.MapGet("/api/tasks/{id}", async (
    int id,
    ClaimsPrincipal user,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);

    var task = await service.GetTaskByIdAsync(
        id,
        userId,
        cancellationToken);

    return Results.Ok(task);
}).RequireAuthorization();

// POST: Create a task for the authenticated user
app.MapPost("/api/tasks", async (
    CreateTaskRequest request,
    ClaimsPrincipal user,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);

    var task = await service.AddTaskAsync(
        request.Title,
        request.Priority,
        userId,
        cancellationToken);

    return Results.Created($"/api/tasks/{task.Id}", task);
}).RequireAuthorization();

// POST: Register (public)
app.MapPost("/api/auth/register", async (
    RegisterRequest request,
    AuthService authService) =>
{
    try
    {
        var user = await authService.RegisterAsync(
            request.Email,
            request.Password);

        return Results.Created(
            $"/api/users/{user.Id}",
            new { user.Id, user.Email });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

// POST: Login (public)
app.MapPost("/api/auth/login", async (
    LoginRequest request,
    AuthService authService) =>
{
    var user = await authService.LoginAsync(
        request.Email,
        request.Password);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        user.Id,
        user.Email,
        token = authService.GenerateToken(user)
    });
}).RequireRateLimiting("login");


// PUT: Complete a task belonging to the authenticated user
app.MapPut("/api/tasks/{id}/complete", async (
    int id,
    ClaimsPrincipal user,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);

    await service.CompleteAsync(
        id,
        userId,
        cancellationToken);

    return Results.NoContent();
}).RequireAuthorization();

// DELETE: Delete a task belonging to the authenticated user
app.MapDelete("/api/tasks/{id}", async (
    int id,
    ClaimsPrincipal user,
    TaskService service,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);

    await service.DeleteTaskAsync(
        id,
        userId,
        cancellationToken);

    return Results.NoContent();
}).RequireAuthorization();

app.Run();
