
using System.Linq;
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

ITaskRepository repository = new InMemoryTaskRepository();
var taskService = new TaskService(repository);

const int userId = 1;

// Add 5 tasks
await taskService.AddTaskAsync("Learn C#", Priority.High, userId);
await taskService.AddTaskAsync("Practice SQL", Priority.Medium, userId);
await taskService.AddTaskAsync("Build API", Priority.High, userId);
await taskService.AddTaskAsync("Learn Angular", Priority.Low, userId);
await taskService.AddTaskAsync("Practice Git", Priority.Medium, userId);

// Complete a task
const int taskId = 1;
await taskService.CompleteAsync(taskId, userId);

// Test custom exception with a different user ID
try
{
    await taskService.CompleteAsync(taskId, 99);
}
catch (TaskNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}

// Group tasks by priority
var tasks = await repository.GetAllAsync(userId);

var groups = tasks
    .GroupBy(task => task.Priority)
    .OrderBy(group => group.Key);

foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}: {group.Count()} tasks");
}

// Test updating a nonexistent task
try
{
    await repository.UpdateAsync(
        new TaskItem(99, "Test task", Priority.Low, true, userId),
        userId
    );
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}

// Get task by ID
var taskOne = await repository.GetByIdAsync(1, userId);

Console.WriteLine($"Task 1 still exists: {taskOne is not null}");
Console.WriteLine($"Task 1 completed: {taskOne?.IsDone}");
