using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Services.DTOs.Task;
using TaskManagementSystem.Services.Interfaces;
using TaskManagementSystem.Core.Exceptions;

namespace TaskManagementSystem.Services.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ICacheService _cacheService;

        public TaskService(IUnitOfWork unitOfWork, IBackgroundTaskQueue taskQueue, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _taskQueue = taskQueue;
            _cacheService = cacheService;
        }

        public async Task<TaskItemDto> CreateTaskAsync(int userId, CreateTaskDto request)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var userRepo = _unitOfWork.Repository<User>();

            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Prevent duplicate tasks: same title, same user, same calendar day
            var today = DateTime.UtcNow.Date;
            var duplicate = await taskRepo.ExistsAsync(t =>
                t.UserId == userId &&
                t.Title == request.Title &&
                t.CreatedAT.Date == today &&
                !t.IsDeleted);

            if (duplicate)
                throw new ConflictException($"A task with the title '{request.Title}' already exists for today.");

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
            var cacheKey = $"task:{taskId}:user:{userId}";
            var cachedTask = await _cacheService.GetAsync<TaskItemDto>(cacheKey);
            if (cachedTask != null)
                return cachedTask;

            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var task = await taskRepo.GetByConditionAsync(t => t.ID == taskId && t.UserId == userId && !t.IsDeleted);

            if (task == null)
                return null;

            var dto = MapToDto(task);

            // Cache the result for 10 minutes
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

            return dto;
        }

        public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync(int userId)
        {
            var taskRepo = _unitOfWork.Repository<TaskItem>();

            // Sort: highest priority first, then oldest creation date first
            var tasks = await taskRepo.GetAllWithOptionsAsync(
                predicate: t => t.UserId == userId && !t.IsDeleted,
                orderBy: q => q.OrderByDescending(t => t.Priority).ThenBy(t => t.CreatedAT));

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
            task.ModifiedBy = userId.ToString();

            taskRepo.Update(task);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync($"task:{taskId}:user:{userId}");
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
