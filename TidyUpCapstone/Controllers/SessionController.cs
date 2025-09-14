using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Models.DTOs.Leaderboard;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly IUserSessionService _userSessionService;

        public SessionController(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Get list of available demo users
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<List<SessionUserDto>>> GetAvailableUsers()
        {
            try
            {
                var users = await _userSessionService.GetAvailableUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get current session user
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<SessionUserDto>> GetCurrentUser()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var user = await _userSessionService.GetUserByIdAsync(userId);

                if (user == null)
                    return NotFound(new { error = "Current user not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Switch to a different demo user
        /// </summary>
        /// <param name="request">User switch request</param>
        [HttpPost("switch")]
        public async Task<ActionResult<SessionUserDto>> SwitchUser([FromBody] SwitchUserRequest request)
        {
            try
            {
                if (request?.UserId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });

                var user = await _userSessionService.GetUserByIdAsync(request.UserId);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                _userSessionService.SetCurrentUserId(HttpContext, request.UserId);

                return Ok(new
                {
                    success = true,
                    message = $"Switched to user: {user.Name}",
                    user = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Initialize demo data (for development/testing)
        /// </summary>
        [HttpPost("init-demo")]
        public async Task<ActionResult> InitializeDemoData()
        {
            try
            {
                await _userSessionService.InitializeDemoUsersAsync();
                return Ok(new { message = "Demo data initialized successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to initialize demo data", details = ex.Message });
            }
        }

        /// <summary>
        /// Clear current session
        /// </summary>
        [HttpPost("clear")]
        public ActionResult ClearSession()
        {
            try
            {
                HttpContext.Session.Clear();
                return Ok(new { message = "Session cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to clear session", details = ex.Message });
            }
        }
    }

    public class SwitchUserRequest
    {
        public int UserId { get; set; }
    }
}