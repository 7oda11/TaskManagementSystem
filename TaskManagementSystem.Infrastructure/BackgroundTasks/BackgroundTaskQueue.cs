using System.Threading.Channels;
using TaskManagementSystem.Core.Interfaces;

namespace TaskManagementSystem.Infrastructure.BackgroundTasks
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<int> _queue;

        public BackgroundTaskQueue()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<int>(options);
        }

        public async ValueTask QueueTaskAsync(int taskId)
        {
            await _queue.Writer.WriteAsync(taskId);
        }

        public async ValueTask<int> DequeueTaskAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
