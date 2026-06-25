using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Core.Interfaces;

namespace TaskManagementSystem.Infrastructure.BackgroundTasks
{
    public class TaskProcessingBackgroundService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskProcessingBackgroundService> _logger;

        public TaskProcessingBackgroundService(
            IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvider,
            ILogger<TaskProcessingBackgroundService> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Processing Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var taskId = await _taskQueue.DequeueTaskAsync(stoppingToken);

                    _logger.LogInformation($"Background Service picked up Task ID: {taskId}. Processing...");

                    await ProcessTaskAsync(taskId, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken is canceled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task.");
                }
            }
        }

        private async Task ProcessTaskAsync(int taskId, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var taskRepo = unitOfWork.Repository<TaskItem>();

            var task = await taskRepo.GetByIdAsync(taskId);
            
            if (task == null || task.IsDeleted)
            {
                _logger.LogWarning($"Task ID {taskId} not found or is deleted. Skipping processing.");
                return;
            }

            // Mark as In Progress
            task.Status = TaskItemStatus.InProgress;
            task.ModifiedAt = DateTime.UtcNow;
            task.ModifiedBy = "BackgroundWorker";
            taskRepo.Update(task);
            await unitOfWork.SaveChangesAsync();

            // Invalidate Cache
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            await cacheService.RemoveAsync($"task:{taskId}:user:{task.UserId}");

            _logger.LogInformation($"Task ID {taskId} marked as InProgress. Simulating work...");

            // Simulate work
            await Task.Delay(5000, stoppingToken);

            // Fetch again to ensure we have the latest state (optional, but good practice)
            task = await taskRepo.GetByIdAsync(taskId);
            if (task != null && !task.IsDeleted)
            {
                // Mark as Done
                task.Status = TaskItemStatus.Done;
                task.ModifiedAt = DateTime.UtcNow;
                task.ModifiedBy = "BackgroundWorker";
                taskRepo.Update(task);
                await unitOfWork.SaveChangesAsync();

                // Invalidate Cache
                await cacheService.RemoveAsync($"task:{taskId}:user:{task.UserId}");

                _logger.LogInformation($"Task ID {taskId} marked as Done. Processing complete.");
            }
        }
    }
}
