
using TaskTracker.Models;
using TaskTracker.Repositories;

namespace TaskTracker.Services;

public class TaskService(ITaskRepository taskRepository)
{
    public async Task<TaskItem> AddTaskAsync(
        string title,
        Priority priority,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var task = new TaskItem(
            Id: 0,
            Title: title,
            Priority: priority,
            IsDone: false
        );

        await taskRepository.AddAsync(task, cancellationToken);

        return task;
    }

    public async Task CompleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            throw new TaskNotFoundException(
                $"Task with ID {id} was not found.");
        }

        var completedTask = task with { IsDone = true };

        await taskRepository.UpdateAsync(completedTask, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllTasksAsync(
        CancellationToken cancellationToken = default)
    {
        return await taskRepository.GetAllAsync(cancellationToken);
    }

    public async Task<TaskItem> GetTaskByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            throw new TaskNotFoundException(
                $"Task with ID {id} was not found.");
        }

        return task;
    }

    public async Task DeleteTaskAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await taskRepository.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            throw new TaskNotFoundException(
                $"Task with ID {id} was not found.");
        }
    }
}
