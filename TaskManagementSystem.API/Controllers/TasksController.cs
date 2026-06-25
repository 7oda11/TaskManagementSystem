using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.Services.DTOs.Task;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="request">The task details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created task.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TaskItemDto>>> CreateTask(
            [FromBody] CreateTaskDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.CreateTaskAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, ApiResponse<TaskItemDto>.Ok(task, "Task created successfully."));
        }

        /// <summary>
        /// Gets a specific task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The task details if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TaskItemDto>>> GetTaskById(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId, cancellationToken);

            if (task == null)
                return NotFound(ApiResponse<TaskItemDto>.Fail("Task not found."));

            return Ok(ApiResponse<TaskItemDto>.Ok(task));
        }

        /// <summary>
        /// Gets all tasks belonging to the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of tasks.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TaskItemDto>>>> GetAllTasks(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetAllTasksAsync(userId, cancellationToken);
            return Ok(ApiResponse<IEnumerable<TaskItemDto>>.Ok(tasks));
        }

        /// <summary>
        /// Updates the status of a specific task.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="request">The new status of the task.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success response.</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTaskStatus(
            int id, [FromBody] UpdateTaskStatusDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _taskService.UpdateTaskStatusAsync(id, userId, request, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Task status updated successfully."));
        }
    }
}
