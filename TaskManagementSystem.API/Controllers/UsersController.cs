using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileDto>> CreateUser([FromBody] CreateUserDto request)
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, user);
        }

        /// <summary>
        /// Soft-deletes a user. Sets IsDeleted = true and records the admin's identity.
        /// The user row is preserved in the database.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var deletedBy = GetCurrentUserEmail();
            await _userService.DeleteUserAsync(id, deletedBy);
            return NoContent();
        }
    }
}
