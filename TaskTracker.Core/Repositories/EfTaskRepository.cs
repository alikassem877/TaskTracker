
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public class EfTaskRepository(AppDbContext context) : ITaskRepository
{

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
    int userId,
    int page,
    int pageSize,
    bool? isDone,
    Priority? priority,
    string? search,
    CancellationToken cancellationToken = default)
{
    var query = context.Tasks
        .AsNoTracking()
        .Where(task => task.UserId == userId);

    if (isDone.HasValue)
        query = query.Where(task => task.IsDone == isDone.Value);

    if (priority.HasValue)
        query = query.Where(task => task.Priority == priority.Value);

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(task => task.Title.Contains(search));

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderBy(task => task.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (items, totalCount);
}

    public async Task<TaskItem?> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                task => task.Id == id && task.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
    TaskItem task,
    int userId,
    CancellationToken cancellationToken = default)
{
    var existingTask = await context.Tasks
        .FirstOrDefaultAsync(
            item => item.Id == task.Id && item.UserId == userId,
            cancellationToken);

    if (existingTask is null)
        return false;

    var updatedTask = existingTask with
    {
        Title = task.Title,
        Priority = task.Priority,
        IsDone = task.IsDone
    };

    context.Entry(existingTask).CurrentValues.SetValues(updatedTask);

    await context.SaveChangesAsync(cancellationToken);

    return true;
}

    public async Task<bool> CompleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await context.Tasks
            .Where(task => task.Id == id && task.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    task => task.IsDone,
                    true),
                cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var task = await context.Tasks
            .FirstOrDefaultAsync(
                item => item.Id == id && item.UserId == userId,
                cancellationToken);

        if (task is null)
            return false;

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
