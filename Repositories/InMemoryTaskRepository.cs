using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly Dictionary<int, TaskItem> _tasks = new();

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
    {
        await Task.Delay(50);

        return _tasks.Values.ToList();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        await Task.Delay(50);

        return _tasks.TryGetValue(id, out var task) ? task : null;
    }

    public async Task AddAsync(TaskItem task)
    {
        await Task.Delay(50);

        _tasks.Add(task.Id, task);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await Task.Delay(50);

        return _tasks.Remove(id);
    }
}