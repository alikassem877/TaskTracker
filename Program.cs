using System.Linq;
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

ITaskRepository repository = new InMemoryTaskRepository();
var taskService = new TaskService(repository);

// Add 5 tasks
await taskService.AddTaskAsync("Learn C#", Priority.High);
await taskService.AddTaskAsync("Practice SQL", Priority.Medium);
await taskService.AddTaskAsync("Build API", Priority.High);
await taskService.AddTaskAsync("Learn Angular", Priority.Low);
await taskService.AddTaskAsync("Practice Git", Priority.Medium);

// Complete a task
await taskService.CompleteAsync(1);

// Test custom exception
try
{
    await taskService.CompleteAsync(99);
}
catch (TaskNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}

// Group tasks by priority
var tasks = await repository.GetAllAsync();

var groups = tasks
    .GroupBy(task => task.Priority)
    .OrderBy(group => group.Key);

foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}: {group.Count()} tasks");
}

try
{
    await repository.UpdateAsync(
        new TaskItem(99, "Test task", Priority.Low, true)
    );
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}

var taskOne = await repository.GetByIdAsync(1);

Console.WriteLine($"Task 1 still exists: {taskOne is not null}");
Console.WriteLine($"Task 1 completed: {taskOne?.IsDone}");