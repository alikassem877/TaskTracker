
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Repositories;

public class EfTaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .AsNoTracking()
            .Where(task => task.UserId == userId)
            .ToListAsync(cancellationToken);
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

    public async Task UpdateAsync(
    TaskItem task,
    int userId,
    CancellationToken cancellationToken = default)
{
    var existingTask = await context.Tasks
        .FirstOrDefaultAsync(
            item => item.Id == task.Id && item.UserId == userId,
            cancellationToken);

    if (existingTask is null)
        return;

    var updatedTask = existingTask with
    {
        Title = task.Title,
        Priority = task.Priority,
        IsDone = task.IsDone
    };

    context.Entry(existingTask).CurrentValues.SetValues(updatedTask);

    await context.SaveChangesAsync(cancellationToken);
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
