using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class MessageController(IMessageService messageService) : BaseApiController
{
    private const int DailyUnmatchedLimit = 5;

    // POST api/message/user  — send to a user (matched or unmatched)
    [HttpPost("user")]
    public async Task<IActionResult> SendToUser([FromBody] SendMessageToUserRequest request)
    {
        try
        {
            var message = await messageService.SendToUserAsync(
                CurrentUserId, request.ReceiverId, request.Content);

            return Ok(new MessageResponse(
                message.Id, message.SenderId, message.Content,
                message.SentAt, message.IsRead));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/message/session/{sessionId}  — send inside a game session
    [HttpPost("session/{sessionId:guid}")]
    public async Task<IActionResult> SendToSession(Guid sessionId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var message = await messageService.SendToSessionAsync(
                CurrentUserId, sessionId, request.Content);

            return Ok(new MessageResponse(
                message.Id, message.SenderId, message.Content,
                message.SentAt, message.IsRead));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/message/conversation/{conversationId}?skip=0&take=50
    [HttpGet("conversation/{conversationId:guid}")]
    public async Task<IActionResult> GetConversationMessages(
        Guid conversationId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            var messages = await messageService.GetConversationMessagesAsync(
                conversationId, skip, take);

            return Ok(messages.Select(m =>
                new MessageResponse(m.Id, m.SenderId, m.Content, m.SentAt, m.IsRead)));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/message/session/{sessionId}
    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSessionMessages(Guid sessionId)
    {
        try
        {
            var messages = await messageService.GetSessionMessagesAsync(sessionId);
            return Ok(messages.Select(m =>
                new MessageResponse(m.Id, m.SenderId, m.Content, m.SentAt, m.IsRead)));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // PATCH api/message/conversation/{conversationId}/read
    [HttpPatch("conversation/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        try
        {
            await messageService.MarkAsReadAsync(conversationId, CurrentUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/message/unmatched/status
    [HttpGet("unmatched/status")]
    public async Task<IActionResult> GetUnmatchedStatus()
    {
        try
        {
            var used = await messageService.GetUnmatchedMessageCountTodayAsync(CurrentUserId);
            var remaining = Math.Max(0, DailyUnmatchedLimit - used);
            return Ok(new UnmatchedMessageStatusResponse(used, DailyUnmatchedLimit, remaining));
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
