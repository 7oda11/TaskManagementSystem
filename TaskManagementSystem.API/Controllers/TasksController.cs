using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.Services.DTOs.Task;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            throw new UnauthorizedAccessException("Invalid or missing user identity in token.");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TaskItemDto>>> CreateTask(
            [FromBody] CreateTaskDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.CreateTaskAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, ApiResponse<TaskItemDto>.Ok(task, "Task created successfully."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TaskItemDto>>> GetTaskById(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId, cancellationToken);

            if (task == null)
                return NotFound(ApiResponse<TaskItemDto>.Fail("Task not found."));

            return Ok(ApiResponse<TaskItemDto>.Ok(task));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TaskItemDto>>>> GetAllTasks(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetAllTasksAsync(userId, cancellationToken);
            return Ok(ApiResponse<IEnumerable<TaskItemDto>>.Ok(tasks));
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTaskStatus(
            int id, [FromBody] UpdateTaskStatusDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _taskService.UpdateTaskStatusAsync(id, userId, request, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Task status updated successfully."));
        }
    }
}
