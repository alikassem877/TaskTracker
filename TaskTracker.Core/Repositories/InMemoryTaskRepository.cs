
using System.Collections.Concurrent;
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<int, TaskItem> _tasks = new();
    private int _nextId;

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TaskItem> tasks = _tasks.Values
            .Where(task => task.UserId == userId)
            .ToList();

        return Task.FromResult(tasks);
    }

    public Task<TaskItem?> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(id, out var task);

        if (task?.UserId != userId)
            task = null;

        return Task.FromResult(task);
    }

    public Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _nextId);

        var newTask = task with { Id = id };

        _tasks.TryAdd(id, newTask);

        return Task.CompletedTask;
    }

   public Task<bool> UpdateAsync(
    TaskItem task,
    int userId,
    CancellationToken cancellationToken = default)
{
    while (_tasks.TryGetValue(task.Id, out var existing))
    {
        if (existing.UserId != userId)
            return Task.FromResult(false);

        var updated = task with { UserId = userId };

        if (_tasks.TryUpdate(task.Id, updated, existing))
            return Task.FromResult(true);
    }

    return Task.FromResult(false);
}

    public Task<bool> CompleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        while (_tasks.TryGetValue(id, out var existing))
        {
            if (existing.UserId != userId)
                return Task.FromResult(false);

            var updated = existing with { IsDone = true };

            if (_tasks.TryUpdate(id, updated, existing))
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (!_tasks.TryGetValue(id, out var task)
            || task.UserId != userId)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_tasks.TryRemove(
            new KeyValuePair<int, TaskItem>(id, task)));
    }
}
