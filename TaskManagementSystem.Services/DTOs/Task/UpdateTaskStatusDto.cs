using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Services.DTOs.Task
{
    public class UpdateTaskStatusDto
    {
        public TaskItemStatus Status { get; set; }
    }
}
