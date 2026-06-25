using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Services.DTOs.Task;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundTaskQueue _taskQueue;

        public TaskService(IUnitOfWork unitOfWork, IBackgroundTaskQueue taskQueue)
        {
            _unitOfWork = unitOfWork;
            _taskQueue = taskQueue;
        }

        public async Task<TaskItemDto> CreateTaskAsync(int userId, CreateTaskDto request)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var userRepo = _unitOfWork.Repository<User>();

            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                UserId = userId,
                CreatedAT = DateTime.UtcNow,
                CreatedBy = user.Email.ToString(),
                ModifiedBy = string.Empty,
                DeletedBy = string.Empty
            };

            await taskRepo.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Queue the task for background processing
            await _taskQueue.QueueTaskAsync(task.ID);

            return MapToDto(task);
        }

        public async Task<TaskItemDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var task = await taskRepo.GetByConditionAsync(t => t.ID == taskId && t.UserId == userId && !t.IsDeleted);

            if (task == null)
                return null;

            return MapToDto(task);
        }

        public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync(int userId)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var tasks = await taskRepo.GetAllByConditionAsync(t => t.UserId == userId && !t.IsDeleted);

            return tasks.Select(MapToDto);
        }

        public async Task UpdateTaskStatusAsync(int taskId, int userId, UpdateTaskStatusDto request)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var task = await taskRepo.GetByConditionAsync(t => t.ID == taskId && t.UserId == userId && !t.IsDeleted);

            if (task == null)
                throw new KeyNotFoundException("Task not found or access denied.");
            var userRepo = _unitOfWork.Repository<User>();

            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            task.Status = request.Status;
            task.ModifiedAt = DateTime.UtcNow;
            task.ModifiedBy = user.Email.ToString();

            taskRepo.Update(task);
            await _unitOfWork.SaveChangesAsync();
        }

        private static TaskItemDto MapToDto(TaskItem task) => new()
        {
            Id = task.ID,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            CreatedAt = task.CreatedAT,
            UserId = task.UserId
        };
    }
}
