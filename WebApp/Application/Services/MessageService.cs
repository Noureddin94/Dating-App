using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class MessageService(
    AppDbContext context,
    IMatchService matchService,
    IGameService gameService) : IMessageService
{
    private const int DailyUnmatchedLimit = 5; // FR-25

    public async Task<Message> SendToUserAsync(string senderId, string receiverId, string content)
    {
        var isMatched = await matchService.IsMatchedAsync(senderId, receiverId);
        var inSession = await gameService.ActiveSessionExistsAsync(senderId, receiverId);

        // Matched users or game session participants message freely (FR-24, FR-27)
        if (!isMatched && !inSession)
        {
            var count = await GetUnmatchedMessageCountTodayAsync(senderId);
            if (count >= DailyUnmatchedLimit)
                throw new InvalidOperationException(
                    $"Daily unmatched message limit of {DailyUnmatchedLimit} reached. Try again tomorrow.");

            await IncrementDailyCountAsync(senderId);
        }

        Guid? conversationId = null;
        if (isMatched)
        {
            var match = await matchService.GetMatchAsync(senderId, receiverId);
            conversationId = match?.Conversation?.Id;
        }

        var message = new Message
        {
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            ConversationId = conversationId,
            // If not matched but in a game session, ConversationId stays null;
            // the front-end uses SendToSessionAsync for in-session chat instead
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> SendToSessionAsync(string senderId, Guid sessionId, string content)
    {
        var session = await context.GameSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Game session not found.");

        if (session.Player1Id != senderId && session.Player2Id != senderId)
            throw new UnauthorizedAccessException("You are not a participant in this session.");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot message in an inactive session.");

        var message = new Message
        {
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            GameSessionId = sessionId
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(
        Guid conversationId, int skip, int take) =>
        await context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

    public async Task<IEnumerable<Message>> GetSessionMessagesAsync(Guid sessionId) =>
        await context.Messages
            .Where(m => m.GameSessionId == sessionId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

    public async Task MarkAsReadAsync(Guid conversationId, string readByUserId)
    {
        var unread = await context.Messages
            .Where(m => m.ConversationId == conversationId
                        && m.SenderId != readByUserId
                        && !m.IsRead)
            .ToListAsync();

        unread.ForEach(m => m.IsRead = true);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetUnmatchedMessageCountTodayAsync(string senderId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await context.DailyActionCounts
            .FirstOrDefaultAsync(d =>
                d.UserId == senderId &&
                d.ActionType == ActionType.UnmatchedMessage &&
                d.Date == today);
        return record?.Count ?? 0;
    }

    private async Task IncrementDailyCountAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await context.DailyActionCounts
            .FirstOrDefaultAsync(d =>
                d.UserId == userId &&
                d.ActionType == ActionType.UnmatchedMessage &&
                d.Date == today);

        if (record is null)
            context.DailyActionCounts.Add(new DailyActionCount
            {
                UserId = userId,
                ActionType = ActionType.UnmatchedMessage,
                Date = today,
                Count = 1
            });
        else
            record.Count++;
    }
}
