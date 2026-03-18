using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Entities;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class MatchController(
    IMatchService matchService,
    IProfileService profileService) : BaseApiController
{
    // GET api/match
    [HttpGet]
    public async Task<IActionResult> GetMatches()
    {
        try
        {
            var matches = await matchService.GetMatchesForUserAsync(CurrentUserId);
            var responses = new List<MatchResponse>();

            foreach (var match in matches)
            {
                var otherUserId = match.User1Id == CurrentUserId ? match.User2Id : match.User1Id;
                var otherProfile = await profileService.GetByUserIdAsync(otherUserId);
                var primaryImage = otherProfile?.ProfileImages
                    .FirstOrDefault(i => i.IsPrimary)?.BlobPath;

                responses.Add(new MatchResponse(
                    match.Id,
                    otherUserId,
                    otherProfile?.FirstName ?? "Unknown",
                    primaryImage,
                    match.Conversation!.Id,
                    match.MatchedAt));
            }

            return Ok(responses);
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/match/{matchId}
    [HttpGet("{matchId:guid}")]
    public async Task<IActionResult> GetMatch(Guid matchId)
    {
        try
        {
            var matches = await matchService.GetMatchesForUserAsync(CurrentUserId);
            var match = matches.FirstOrDefault(m => m.Id == matchId);

            if (match is null) return NotFound(new { error = "Match not found." });

            var otherUserId = match.User1Id == CurrentUserId ? match.User2Id : match.User1Id;
            var otherProfile = await profileService.GetByUserIdAsync(otherUserId);
            var primaryImage = otherProfile?.ProfileImages
                .FirstOrDefault(i => i.IsPrimary)?.BlobPath;

            return Ok(new MatchResponse(
                match.Id,
                otherUserId,
                otherProfile?.FirstName ?? "Unknown",
                primaryImage,
                match.Conversation!.Id,
                match.MatchedAt));
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
