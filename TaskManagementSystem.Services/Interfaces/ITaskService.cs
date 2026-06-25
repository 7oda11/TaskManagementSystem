using TaskManagementSystem.Services.DTOs.Task;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItemDto> CreateTaskAsync(int userId, CreateTaskDto request);
        Task<TaskItemDto?> GetTaskByIdAsync(int taskId, int userId);
        Task<IEnumerable<TaskItemDto>> GetAllTasksAsync(int userId);
        Task UpdateTaskStatusAsync(int taskId, int userId, UpdateTaskStatusDto request);
    }
}
