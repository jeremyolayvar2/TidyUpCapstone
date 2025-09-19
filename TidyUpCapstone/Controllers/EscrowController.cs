using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscrowController : ControllerBase
    {
        private readonly IEscrowService _escrowService;

        public EscrowController(IEscrowService escrowService)
        {
            _escrowService = escrowService;
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmTransaction([FromBody] ConfirmRequest request)
        {
            var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;
            var (success, message) = await _escrowService.ConfirmTransactionAsync(request.TransactionId, currentUserId);

            return Ok(new { success, message, bothConfirmed = message.Contains("completed") });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTransaction([FromBody] CancelRequest request)
        {
            var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;
            var (success, message) = await _escrowService.CancelTransactionAsync(request.TransactionId, currentUserId);

            return Ok(new { success, message });
        }
    }

    public class ConfirmRequest
    {
        public int TransactionId { get; set; }
    }

    public class CancelRequest
    {
        public int TransactionId { get; set; }
        public string Reason { get; set; }
    }
}