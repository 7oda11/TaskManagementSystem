using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Services.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    }
}
