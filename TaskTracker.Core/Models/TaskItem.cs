namespace TaskTracker.Models;

public record TaskItem(
    int Id,
    string Title,
    Priority Priority,
    bool IsDone,
    int UserId
);