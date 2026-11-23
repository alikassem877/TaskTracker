
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.ExceptionHandlers;
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

public class ExceptionHandlerTests
{
    [Fact]
    public async Task ArgumentException_Returns400()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services.AddExceptionHandler<TaskNotFoundExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddScoped<ITaskRepository, FakeRepository>();
        builder.Services.AddScoped<TaskService>();

        await using var app = builder.Build();

        app.UseExceptionHandler();

        app.Run(async context =>
        {
            var service = context.RequestServices
                .GetRequiredService<TaskService>();

            await service.AddTaskAsync(" ", Priority.Low, 1);
        });

        await app.StartAsync();

        using var client = app.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.Equal(
            System.Net.HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private class FakeRepository : ITaskRepository
    {
        public Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
            int userId,
            int page,
            int pageSize,
            bool? isDone,
            Priority? priority,
            string? search,
            CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<TaskItem>, int)>(([], 0));

        public Task<TaskItem?> GetByIdAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<TaskItem?>(null);

        public Task AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> UpdateAsync(
            TaskItem task,
            int userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> CompleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
