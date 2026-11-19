using System;

namespace TaskTracker.Services;

public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(string message)
        : base(message)
    {
    }
}