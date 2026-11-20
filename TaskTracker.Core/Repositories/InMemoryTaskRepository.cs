using System.Collections.Concurrent;
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<int, TaskItem> _tasks = new();

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TaskItem> tasks = _tasks.Values.ToList();
        return Task.FromResult(tasks);
    }

    public Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        if (!_tasks.TryAdd(task.Id, task))
        {
            throw new InvalidOperationException(
                $"Task with ID {task.Id} already exists."
            );
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        if (!_tasks.TryGetValue(task.Id, out var existing))
        {
            throw new KeyNotFoundException(
                $"Task with ID {task.Id} was not found."
            );
        }

        if (!_tasks.TryUpdate(task.Id, task, existing))
        {
            throw new InvalidOperationException(
                $"Task with ID {task.Id} was modified concurrently."
            );
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tasks.TryRemove(id, out _));
    }
}