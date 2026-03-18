using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class GameController(IGameService gameService) : BaseApiController
{
    // POST api/game/invite
    [HttpPost("invite")]
    public async Task<IActionResult> SendInvite([FromBody] SendGameInviteRequest request)
    {
        try
        {
            var invite = await gameService.SendInviteAsync(
                CurrentUserId, request.ReceiverId, request.GameType);

            return Ok(MapInviteResponse(invite));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/game/invites/pending
    [HttpGet("invites/pending")]
    public async Task<IActionResult> GetPendingInvites()
    {
        try
        {
            var invites = await gameService.GetPendingInvitesAsync(CurrentUserId);
            return Ok(invites.Select(MapInviteResponse));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/game/invite/{inviteId}/accept
    [HttpPost("invite/{inviteId:guid}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid inviteId)
    {
        try
        {
            var session = await gameService.AcceptInviteAsync(inviteId, CurrentUserId);
            return Ok(MapSessionResponse(session));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/game/invite/{inviteId}/decline
    [HttpPost("invite/{inviteId:guid}/decline")]
    public async Task<IActionResult> DeclineInvite(Guid inviteId)
    {
        try
        {
            await gameService.DeclineInviteAsync(inviteId, CurrentUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/game/sessions/active
    [HttpGet("sessions/active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        try
        {
            var sessions = await gameService.GetActiveSessionsAsync(CurrentUserId);
            return Ok(sessions.Select(MapSessionResponse));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/game/session/{sessionId}
    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        try
        {
            var session = await gameService.GetSessionAsync(sessionId);
            if (session is null) return NotFound(new { error = "Session not found." });

            if (session.Player1Id != CurrentUserId && session.Player2Id != CurrentUserId)
                return Forbid();

            return Ok(MapSessionResponse(session));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // PATCH api/game/session/{sessionId}/state
    [HttpPatch("session/{sessionId:guid}/state")]
    public async Task<IActionResult> UpdateState(
        Guid sessionId, [FromBody] UpdateGameStateRequest request)
    {
        try
        {
            var session = await gameService.GetSessionAsync(sessionId);
            if (session is null) return NotFound(new { error = "Session not found." });

            if (session.Player1Id != CurrentUserId && session.Player2Id != CurrentUserId)
                return Forbid();

            await gameService.UpdateSessionStateAsync(sessionId, request.StateJson);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/game/session/{sessionId}/end
    [HttpPost("session/{sessionId:guid}/end")]
    public async Task<IActionResult> EndSession(Guid sessionId)
    {
        try
        {
            var session = await gameService.GetSessionAsync(sessionId);
            if (session is null) return NotFound(new { error = "Session not found." });

            if (session.Player1Id != CurrentUserId && session.Player2Id != CurrentUserId)
                return Forbid();

            await gameService.EndSessionAsync(sessionId, SessionStatus.Completed);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static GameInviteResponse MapInviteResponse(Domain.Entities.GameInvite i) => new(
        i.Id,
        i.SenderId,
        i.Sender?.UserName ?? string.Empty,
        i.ReceiverId,
        i.GameType,
        i.Status,
        i.ExpiresAt,
        i.CreatedAt);

    private static GameSessionResponse MapSessionResponse(Domain.Entities.GameSession s) => new(
        s.Id,
        s.GameType,
        s.Player1Id,
        s.Player2Id,
        s.StateJson,
        s.Status,
        s.StartedAt);
}
