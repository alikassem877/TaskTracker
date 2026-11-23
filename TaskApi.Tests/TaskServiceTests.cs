
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task AddTaskAsync_ValidTask_AddsTaskToRepository()
    {
        var repository = new FakeTaskRepository();
        var service = new TaskService(repository);

        var result = await service.AddTaskAsync("Learn C#", Priority.High, 1);

        Assert.Equal("Learn C#", result.Title);
        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal(1, result.UserId);
        Assert.Single(repository.Tasks);
    }

    [Fact]
    public async Task AddTaskAsync_EmptyTitle_ThrowsArgumentException()
    {
        var repository = new FakeTaskRepository();
        var service = new TaskService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddTaskAsync("", Priority.High, 1));

        Assert.Empty(repository.Tasks);
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskNotFound_ThrowsTaskNotFoundException()
    {
        var service = new TaskService(new FakeTaskRepository());

        await Assert.ThrowsAsync<TaskNotFoundException>(
            () => service.GetTaskByIdAsync(999, 1));
    }

    [Fact]
    public async Task GetTaskByIdAsync_OtherUsersTask_ThrowsTaskNotFoundException()
    {
        var repository = new FakeTaskRepository();

        repository.Tasks.Add(new TaskItem(
            1, "Private task", Priority.High, false, 2));

        var service = new TaskService(repository);

        await Assert.ThrowsAsync<TaskNotFoundException>(
            () => service.GetTaskByIdAsync(1, 1));
    }

    [Fact]
    public async Task GetTaskByIdAsync_ExistingTask_ReturnsTask()
    {
        var repository = new FakeTaskRepository();
        var expectedTask = new TaskItem(
            1, "Learn ASP.NET Core", Priority.High, false, 1);

        repository.Tasks.Add(expectedTask);

        var service = new TaskService(repository);
        var result = await service.GetTaskByIdAsync(1, 1);

        Assert.Equal(expectedTask, result);
    }

    [Fact]
    public async Task CompleteAsync_ExistingTask_MarksTaskAsDone()
    {
        var repository = new FakeTaskRepository();

        repository.Tasks.Add(new TaskItem(
            1, "Learn testing", Priority.High, false, 1));

        var service = new TaskService(repository);

        await service.CompleteAsync(1, 1);

        Assert.True(repository.Tasks[0].IsDone);
    }

    [Fact]
    public async Task CompleteAsync_TaskNotFound_ThrowsTaskNotFoundException()
    {
        var service = new TaskService(new FakeTaskRepository());

        await Assert.ThrowsAsync<TaskNotFoundException>(
            () => service.CompleteAsync(999, 1));
    }

    [Fact]
    public async Task DeleteTaskAsync_ExistingTask_RemovesTask()
    {
        var repository = new FakeTaskRepository();

        repository.Tasks.Add(new TaskItem(
            1, "Temporary task", Priority.Low, false, 1));

        var service = new TaskService(repository);

        await service.DeleteTaskAsync(1, 1);

        Assert.Empty(repository.Tasks);
    }

    [Fact]
    public async Task DeleteTaskAsync_TaskNotFound_ThrowsTaskNotFoundException()
    {
        var service = new TaskService(new FakeTaskRepository());

        await Assert.ThrowsAsync<TaskNotFoundException>(
            () => service.DeleteTaskAsync(999, 1));
    }

    [Fact]
    public async Task UpdateTaskAsync_ExistingTask_UpdatesTask()
    {
        var repository = new FakeTaskRepository();

        repository.Tasks.Add(new TaskItem(
            1, "Old title", Priority.Low, false, 1));

        var service = new TaskService(repository);
        var updatedTask = new TaskItem(
            1, "New title", Priority.High, false, 1);

        await service.UpdateTaskAsync(updatedTask, 1);

        Assert.Equal("New title", repository.Tasks[0].Title);
        Assert.Equal(Priority.High, repository.Tasks[0].Priority);
    }

    [Fact]
    public async Task UpdateTaskAsync_TaskNotFound_ThrowsTaskNotFoundException()
    {
        var service = new TaskService(new FakeTaskRepository());
        var task = new TaskItem(
            999, "Missing task", Priority.High, false, 1);

        await Assert.ThrowsAsync<TaskNotFoundException>(
            () => service.UpdateTaskAsync(task, 1));
    }

    private class FakeTaskRepository : ITaskRepository
    {
        public List<TaskItem> Tasks { get; } = [];

        public Task AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
        {
            Tasks.Add(task);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
            int userId,
            int page,
            int pageSize,
            bool? isDone,
            Priority? priority,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var items = Tasks
                .Where(t => t.UserId == userId)
                .ToList();

            return Task.FromResult<(IReadOnlyList<TaskItem>, int)>(
                (items, items.Count));
        }

        public Task<TaskItem?> GetByIdAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var task = Tasks.FirstOrDefault(
                t => t.Id == id && t.UserId == userId);

            return Task.FromResult(task);
        }

        public Task<bool> UpdateAsync(
            TaskItem task,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var index = Tasks.FindIndex(
                t => t.Id == task.Id && t.UserId == userId);

            if (index == -1)
                return Task.FromResult(false);

            Tasks[index] = task;
            return Task.FromResult(true);
        }

        public Task<bool> CompleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var task = Tasks.FirstOrDefault(
                t => t.Id == id && t.UserId == userId);

            if (task is null)
                return Task.FromResult(false);

            var index = Tasks.IndexOf(task);
            Tasks[index] = task with { IsDone = true };

            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var task = Tasks.FirstOrDefault(
                t => t.Id == id && t.UserId == userId);

            if (task is null)
                return Task.FromResult(false);

            Tasks.Remove(task);
            return Task.FromResult(true);
        }
    }
}
