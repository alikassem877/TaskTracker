using TaskTracker.Models;

namespace TaskTracker.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync();

    Task<TaskItem?> GetByIdAsync(int id);

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task<bool> DeleteAsync(int id);
}