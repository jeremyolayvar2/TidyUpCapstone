using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;
        private readonly IUserSessionService _userSessionService;

        public GamificationController(
            IGamificationService gamificationService,
            IUserSessionService userSessionService)
        {
            _gamificationService = gamificationService;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Get all quests for the current user
        /// </summary>
        [HttpGet("quests")]
        public async Task<ActionResult<List<QuestDto>>> GetUserQuests()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var quests = await _gamificationService.GetUserQuestsAsync(userId);
                return Ok(quests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get quests for a specific user
        /// </summary>
        [HttpGet("quests/{userId:int}")]
        public async Task<ActionResult<List<QuestDto>>> GetUserQuests(int userId)
        {
            try
            {
                var quests = await _gamificationService.GetUserQuestsAsync(userId);
                return Ok(quests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all achievements for the current user
        /// </summary>
        [HttpGet("achievements")]
        public async Task<ActionResult<List<AchievementDto>>> GetUserAchievements()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var achievements = await _gamificationService.GetUserAchievementsAsync(userId);
                return Ok(achievements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get achievements for a specific user
        /// </summary>
        [HttpGet("achievements/{userId:int}")]
        public async Task<ActionResult<List<AchievementDto>>> GetUserAchievements(int userId)
        {
            try
            {
                var achievements = await _gamificationService.GetUserAchievementsAsync(userId);
                return Ok(achievements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get leaderboard data
        /// </summary>
        [HttpGet("leaderboard/{leaderboardId:int}")]
        public async Task<ActionResult<LeaderboardDto>> GetLeaderboard(int leaderboardId)
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var leaderboard = await _gamificationService.GetLeaderboardAsync(leaderboardId, userId);

                if (leaderboard.LeaderboardId == 0)
                    return NotFound(new { error = "Leaderboard not found" });

                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Complete a quest
        /// </summary>
        [HttpPost("quests/{questId:int}/complete")]
        public async Task<ActionResult> CompleteQuest(int questId)
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var success = await _gamificationService.CompleteUserQuestAsync(userId, questId);

                if (!success)
                    return BadRequest(new { error = "Quest cannot be completed or is already completed" });

                return Ok(new { message = "Quest completed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Claim quest reward
        /// </summary>
        [HttpPost("quests/{userQuestId:int}/claim")]
        public async Task<ActionResult> ClaimQuestReward(int userQuestId)
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId(HttpContext);
                var success = await _gamificationService.ClaimQuestRewardAsync(userId, userQuestId);

                if (!success)
                    return BadRequest(new { error = "Reward cannot be claimed or is already claimed" });

                return Ok(new { message = "Reward claimed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}