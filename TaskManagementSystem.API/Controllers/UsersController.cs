using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private string GetCurrentUserEmail()
            => User.FindFirstValue(ClaimTypes.Email)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? "Unknown";

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserProfileDto>>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(ApiResponse<IEnumerable<UserProfileDto>>.Ok(users));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> CreateUser(
            [FromBody] CreateUserDto request, CancellationToken cancellationToken)
        {
            var user = await _userService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, ApiResponse<UserProfileDto>.Ok(user, "User created successfully."));
        }

        /// <summary>
        /// Soft-deletes a user. Sets IsDeleted = true and records the admin's identity.
        /// The user row is preserved in the database.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var deletedBy = GetCurrentUserEmail();
            await _userService.DeleteUserAsync(id, deletedBy, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "User deleted successfully."));
        }
    }
}
