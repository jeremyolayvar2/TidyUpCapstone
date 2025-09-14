using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Models.DTOs.Leaderboard;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardApiController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardApiController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Get leaderboard data for specified filter type
        /// </summary>
        /// <param name="filterType">all-time, weekly, or daily</param>
        [HttpGet("{filterType}")]
        public async Task<ActionResult<LeaderboardResponseDto>> GetLeaderboard(string filterType)
        {
            try
            {
                var filter = ParseFilterType(filterType);
                var leaderboard = await _leaderboardService.GetLeaderboardAsync(filter);

                return Ok(leaderboard);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get specific user's stats
        /// </summary>
        /// <param name="userId">User ID</param>
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats(int userId)
        {
            try
            {
                var stats = await _leaderboardService.GetUserStatsAsync(userId);

                if (stats.UserId == 0)
                    return NotFound(new { error = "User not found" });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get top users
        /// </summary>
        /// <param name="count">Number of top users to return (default: 10)</param>
        [HttpGet("top")]
        public async Task<ActionResult<List<UserStatsDto>>> GetTopUsers([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 100)
                    count = 10;

                var topUsers = await _leaderboardService.GetTopUsersAsync(count);
                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        private LeaderboardFilterType ParseFilterType(string filterType)
        {
            return filterType?.ToLower() switch
            {
                "all-time" => LeaderboardFilterType.AllTime,
                "weekly" => LeaderboardFilterType.Weekly,
                "daily" => LeaderboardFilterType.Daily,
                _ => throw new ArgumentException($"Invalid filter type: {filterType}. Valid types are: all-time, weekly, daily")
            };
        }
    }
}