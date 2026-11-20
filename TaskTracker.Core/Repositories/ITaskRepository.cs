
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TaskItem task,
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);
}
