
using TaskTracker.Models;
using TaskTracker.Repositories;

namespace TaskTracker.Services;

public class TaskService(ITaskRepository taskRepository)
{
    public async Task<TaskItem> AddTaskAsync(
        string title,
        Priority priority,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var task = new TaskItem(
            Id: 0,
            Title: title,
            Priority: priority,
            IsDone: false,
            UserId: userId
        );

        await taskRepository.AddAsync(task, cancellationToken);

        return task;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllTasksAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await taskRepository.GetAllAsync(
            userId, cancellationToken);
    }

    public async Task<TaskItem> GetTaskByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(
            id, userId, cancellationToken);

        return task ?? throw new TaskNotFoundException(
            $"Task with ID {id} was not found.");
    }

    public async Task CompleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var completed = await taskRepository.CompleteAsync(
            id, userId, cancellationToken);

        if (!completed)
            throw new TaskNotFoundException(
                $"Task with ID {id} was not found.");
    }

    public async Task DeleteTaskAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var deleted = await taskRepository.DeleteAsync(
            id, userId, cancellationToken);

        if (!deleted)
            throw new TaskNotFoundException(
                $"Task with ID {id} was not found.");
    }

    public async Task UpdateTaskAsync(
    TaskItem task,
    int userId,
    CancellationToken cancellationToken = default)
{
    var updated = await taskRepository.UpdateAsync(
        task, userId, cancellationToken);

    if (!updated)
        throw new TaskNotFoundException(
            $"Task with ID {task.Id} was not found.");
}
}
