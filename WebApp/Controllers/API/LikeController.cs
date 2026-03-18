using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class LikeController(
    ILikeService likeService,
    IMatchService matchService) : BaseApiController
{
    private const int DailyLikeLimit = 20;

    // POST api/like
    [HttpPost]
    public async Task<IActionResult> SendLike([FromBody] SendLikeRequest request)
    {
        try
        {
            var like = await likeService.SendAsync(CurrentUserId, request.ReceiverId, request.IsLike);

            // Immediately check for a match after every like
            var match = request.IsLike
                ? await matchService.TryCreateMatchAsync(CurrentUserId, request.ReceiverId)
                : null;

            return Ok(new LikeResponse(
                like.Id,
                like.SenderId,
                like.ReceiverId,
                like.IsLike,
                like.CreatedAt,
                MatchCreated: match is not null));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/like/status
    [HttpGet("status")]
    public async Task<IActionResult> GetDailyStatus()
    {
        try
        {
            var used = await likeService.GetDailyLikeCountAsync(CurrentUserId);
            var remaining = Math.Max(0, DailyLikeLimit - used);
            return Ok(new DailyLikeStatusResponse(used, DailyLikeLimit, remaining));
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
