using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Core.Aggregates
{
    public class TaskItem : BaseModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
