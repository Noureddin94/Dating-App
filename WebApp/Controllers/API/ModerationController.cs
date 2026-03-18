using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class ModerationController(IModerationService moderationService) : BaseApiController
{
    // POST api/moderation/block
    [HttpPost("block")]
    public async Task<IActionResult> BlockUser([FromBody] string blockedUserId)
    {
        try
        {
            if (blockedUserId == CurrentUserId)
                return BadRequest(new { error = "You cannot block yourself." });

            await moderationService.BlockUserAsync(CurrentUserId, blockedUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // DELETE api/moderation/block/{blockedUserId}
    [HttpDelete("block/{blockedUserId}")]
    public async Task<IActionResult> UnblockUser(string blockedUserId)
    {
        try
        {
            await moderationService.UnblockUserAsync(CurrentUserId, blockedUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/moderation/report
    [HttpPost("report")]
    public async Task<IActionResult> ReportUser([FromBody] ReportUserRequest request)
    {
        try
        {
            if (request.ReportedUserId == CurrentUserId)
                return BadRequest(new { error = "You cannot report yourself." });

            await moderationService.ReportUserAsync(
                CurrentUserId, request.ReportedUserId, request.Reason);

            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
