using System;
using System.Threading.Tasks;
using TaskTracker.Models;
using TaskTracker.Repositories;

namespace TaskTracker.Services;

public class TaskService(ITaskRepository taskRepository)
{
    private int _nextId = 1;

    public async Task AddTaskAsync(string title, Priority priority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var task = new TaskItem(
            Id: _nextId++,
            Title: title,
            Priority: priority,
            IsDone: false
        );

        await taskRepository.AddAsync(task);
    }

    public async Task CompleteAsync(int id)
    {
        var task = await taskRepository.GetByIdAsync(id);

        if (task is null)
        {
            throw new TaskNotFoundException($"Task with ID {id} was not found.");
        }

        var completedTask = task with { IsDone = true };

        await taskRepository.DeleteAsync(id);
        await taskRepository.AddAsync(completedTask);
    }

    
};