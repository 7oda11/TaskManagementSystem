namespace TaskManagementSystem.Core.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueTaskAsync(int taskId);
        ValueTask<int> DequeueTaskAsync(CancellationToken cancellationToken);
    }
}
