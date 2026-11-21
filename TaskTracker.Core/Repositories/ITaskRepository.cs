
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public interface ITaskRepository
{

    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
    int userId,
    int page,
    int pageSize,
    bool? isDone,
    Priority? priority,
    string? search,
    CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
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
