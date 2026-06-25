using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            IUnitOfWork unitOfWork,
            IBackgroundTaskQueue taskQueue,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<TaskService> logger)
        {
            _unitOfWork = unitOfWork;
            _taskQueue = taskQueue;
            _cacheService = cacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TaskItemDto> CreateTaskAsync(int userId, CreateTaskDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create task '{Title}' for user ID: {UserId}", request.Title, userId);
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var userRepo = _unitOfWork.Repository<User>();

            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Task creation failed. User ID: {UserId} not found.", userId);
                throw new KeyNotFoundException("User not found.");
            }

            // Prevent duplicate tasks: same title, same user, same calendar day
            var today = DateTime.UtcNow.Date;
            var duplicate = await taskRepo.ExistsAsync(t =>
                t.UserId == userId &&
                t.Title == request.Title &&
                t.CreatedAT.Date == today &&
                !t.IsDeleted, cancellationToken);

            if (duplicate)
            {
                _logger.LogWarning("Task creation failed for user ID: {UserId}. Duplicate task title '{Title}' for today.", userId, request.Title);
                throw new ConflictException($"A task with the title '{request.Title}' already exists for today.");
            }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                UserId = userId,
                CreatedAT = DateTime.UtcNow,
                CreatedBy = user.Email,
                ModifiedBy = string.Empty,
                DeletedBy = string.Empty
            };

            await taskRepo.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task created successfully with ID: {TaskId} for user ID: {UserId}", task.ID, userId);

            // Queue the task for background processing
            await _taskQueue.QueueTaskAsync(task.ID);
            _logger.LogInformation("Task ID: {TaskId} queued for background processing", task.ID);

            return _mapper.Map<TaskItemDto>(task);
        }

        public async Task<TaskItemDto?> GetTaskByIdAsync(int taskId, int userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving task ID: {TaskId} for user ID: {UserId}", taskId, userId);
            var cacheKey = $"task:{taskId}:user:{userId}";
            var cachedTask = await _cacheService.GetAsync<TaskItemDto>(cacheKey, cancellationToken);
            if (cachedTask != null)
            {
                _logger.LogInformation("Task ID: {TaskId} retrieved from cache.", taskId);
                return cachedTask;
            }

            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var task = await taskRepo.GetByConditionAsync(t => t.ID == taskId && t.UserId == userId && !t.IsDeleted, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task ID: {TaskId} for user ID: {UserId} not found.", taskId, userId);
                return null;
            }

            var dto = _mapper.Map<TaskItemDto>(task);

            // Cache the result for 10 minutes
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);
            _logger.LogInformation("Task ID: {TaskId} retrieved from database and cached.", taskId);

            return dto;
        }

        public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync(int userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all tasks for user ID: {UserId}", userId);
            var taskRepo = _unitOfWork.Repository<TaskItem>();

            // Sort: highest priority first, then oldest creation date first
            var tasks = await taskRepo.GetAllWithOptionsAsync(
                predicate: t => t.UserId == userId && !t.IsDeleted,
                orderBy: q => q.OrderByDescending(t => t.Priority).ThenBy(t => t.CreatedAT),
                cancellationToken: cancellationToken);

            return _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
        }

        public async Task UpdateTaskStatusAsync(int taskId, int userId, UpdateTaskStatusDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update status for task ID: {TaskId} by user ID: {UserId} to {Status}", taskId, userId, request.Status);
            var taskRepo = _unitOfWork.Repository<TaskItem>();
            var task = await taskRepo.GetByConditionAsync(t => t.ID == taskId && t.UserId == userId && !t.IsDeleted, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Status update failed. Task ID: {TaskId} not found or access denied.", taskId);
                throw new KeyNotFoundException("Task not found or access denied.");
            }
            var userRepo = _unitOfWork.Repository<User>();

            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Status update failed. User ID: {UserId} not found.", userId);
                throw new KeyNotFoundException("User not found.");
            }

            task.Status = request.Status;
            task.ModifiedAt = DateTime.UtcNow;
            task.ModifiedBy = userId.ToString();

            taskRepo.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Status for task ID: {TaskId} successfully updated to {Status}", taskId, request.Status);

            // Invalidate cache
            await _cacheService.RemoveAsync($"task:{taskId}:user:{userId}", cancellationToken);
            _logger.LogInformation("Cache invalidated for task ID: {TaskId} and user ID: {UserId}", taskId, userId);
        }
    }
}
