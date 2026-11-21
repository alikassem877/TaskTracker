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
        using var server = new TestServer(
            new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddExceptionHandler<TaskNotFoundExceptionHandler>();
                    services.AddProblemDetails();
                    services.AddScoped<ITaskRepository, FakeRepository>();
                    services.AddScoped<TaskService>();
                })
                .Configure(app =>
                {
                    app.UseExceptionHandler();

                    app.Run(async context =>
                    {
                        var service = context.RequestServices
                            .GetRequiredService<TaskService>();

                        await service.AddTaskAsync(" ", Priority.Low, 1);
                    });
                }));

        using var client = server.CreateClient();

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
        {
            return Task.FromResult<(IReadOnlyList<TaskItem>, int)>(([], 0));
        }

        public Task<TaskItem?> GetByIdAsync(
            int id, int userId, CancellationToken cancellationToken = default)
            => Task.FromResult<TaskItem?>(null);

        public Task AddAsync(
            TaskItem task, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> UpdateAsync(
            TaskItem task, int userId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> CompleteAsync(
            int id, int userId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> DeleteAsync(
            int id, int userId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}