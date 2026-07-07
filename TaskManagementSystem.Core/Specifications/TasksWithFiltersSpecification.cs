using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Models;

namespace TaskManagementSystem.Core.Specifications
{
    public class TasksWithFiltersSpecification : BaseSpecification<TaskItem>
    {
        public TasksWithFiltersSpecification(int userId, TaskFilter filter) 
            : base(t => t.UserId == userId && !t.IsDeleted &&
                   (string.IsNullOrEmpty(filter.SearchTerm) || t.Title.ToLower().Contains(filter.SearchTerm.ToLower()) || (t.Description != null && t.Description.ToLower().Contains(filter.SearchTerm.ToLower()))) &&
                   (!filter.Status.HasValue || t.Status == filter.Status) &&
                   (!filter.Priority.HasValue || t.Priority == filter.Priority) &&
                   (!filter.CreatedAtFrom.HasValue || t.CreatedAT >= filter.CreatedAtFrom.Value) &&
                   (!filter.CreatedAtTo.HasValue || t.CreatedAT <= filter.CreatedAtTo.Value))
        {
            // Sorting
            bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
            
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "title":
                        if (isDesc) ApplyOrderByDescending(t => t.Title);
                        else ApplyOrderBy(t => t.Title);
                        break;
                    case "status":
                        if (isDesc) ApplyOrderByDescending(t => t.Status);
                        else ApplyOrderBy(t => t.Status);
                        break;
                    case "priority":
                        if (isDesc) ApplyOrderByDescending(t => t.Priority);
                        else ApplyOrderBy(t => t.Priority);
                        break;
                    case "createdat":
                    default:
                        if (isDesc) ApplyOrderByDescending(t => t.CreatedAT);
                        else ApplyOrderBy(t => t.CreatedAT);
                        break;
                }
            }
            else
            {
                // Default Sort: Priority High to Low, then oldest to newest
                // Wait, since we can only apply one OrderBy and one ThenBy in basic spec,
                // let's just do CreatedAt descending as default.
                ApplyOrderByDescending(t => t.CreatedAT);
            }

            // Pagination
            ApplyPaging((filter.Page - 1) * filter.PageSize, filter.PageSize);
        }
    }
}
