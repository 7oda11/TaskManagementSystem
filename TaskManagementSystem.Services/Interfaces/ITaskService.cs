using TaskManagementSystem.Services.DTOs.Task;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItemDto> CreateTaskAsync(int userId, CreateTaskDto request, CancellationToken cancellationToken = default);
        Task<TaskItemDto?> GetTaskByIdAsync(int taskId, int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskItemDto>> GetAllTasksAsync(int userId, CancellationToken cancellationToken = default);
        Task UpdateTaskStatusAsync(int taskId, int userId, UpdateTaskStatusDto request, CancellationToken cancellationToken = default);
    }
}
