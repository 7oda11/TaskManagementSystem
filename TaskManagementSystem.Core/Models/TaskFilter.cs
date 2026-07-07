using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Core.Models
{
    public class TaskFilter
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } // "asc" or "desc"

        // Filters
        public string? SearchTerm { get; set; } // e.g., for Title or Description
        public TaskItemStatus? Status { get; set; }
        public TaskItemPriority? Priority { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreatedAtTo { get; set; }
    }
}
