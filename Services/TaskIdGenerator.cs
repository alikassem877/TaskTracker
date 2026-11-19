namespace TaskTracker.Services;

public class TaskIdGenerator
{
    private int _nextId = 0;

    public int GetNextId()
    {
        return Interlocked.Increment(ref _nextId);
    }
}