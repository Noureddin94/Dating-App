using WebApp.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface IMessageService
{
    /// <summary>
    /// Sends a message to a matched user (unlimited) or an unmatched user
    /// (max 5/day per FR-25). Also allowed inside an active GameSession.
    /// Throws InvalidOperationException if the unmatched daily limit is reached.
    /// </summary>
    Task<Message> SendToUserAsync(string senderId, string receiverId, string content);

    /// <summary>Sends a message inside an active game session chat.</summary>
    Task<Message> SendToSessionAsync(string senderId, Guid sessionId, string content);

    Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int skip, int take);
    Task<IEnumerable<Message>> GetSessionMessagesAsync(Guid sessionId);
    Task MarkAsReadAsync(Guid conversationId, string readByUserId);
    Task<int> GetUnmatchedMessageCountTodayAsync(string senderId);
}
