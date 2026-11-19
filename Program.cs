using System.Linq;
using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

ITaskRepository repository = new InMemoryTaskRepository();
var service = new TaskService(repository);

// Add 5 tasks
await service.AddTaskAsync("Learn C#", Priority.High);
await service.AddTaskAsync("Practice SQL", Priority.Medium);
await service.AddTaskAsync("Build API", Priority.High);
await service.AddTaskAsync("Learn Angular", Priority.Low);
await service.AddTaskAsync("Practice Git", Priority.Medium);

// Complete a task
await service.CompleteAsync(1);

// Test custom exception
try
{
    await service.CompleteAsync(99);
}
catch (TaskNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}

// Group tasks by priority
var tasks = await repository.GetAllAsync();

var groups = tasks.GroupBy(task => task.Priority);

foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}: {group.Count()} tasks");
}