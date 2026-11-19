using TaskTracker.Models;
using TaskTracker.Repositories;

namespace TaskTracker.Services;

public class TaskService(
    ITaskRepository taskRepository,
    TaskIdGenerator idGenerator)
{

    public async Task<TaskItem> AddTaskAsync(string title, Priority priority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var task = new TaskItem(
            Id: idGenerator.GetNextId(),
            Title: title,
            Priority: priority,
            IsDone: false
        );

        await taskRepository.AddAsync(task);

        return task;
    }

    public async Task CompleteAsync(int id)
    {
        var task = await taskRepository.GetByIdAsync(id);

        if (task is null)
        {
            throw new TaskNotFoundException($"Task with ID {id} was not found.");
        }

        var completedTask = task with { IsDone = true };

        await taskRepository.UpdateAsync(completedTask);
    }
}