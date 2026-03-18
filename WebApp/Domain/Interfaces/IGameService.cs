using WebApp.Domain.Entities;
using WebApp.Domain.Enums;

namespace WebApp.Domain.Interfaces;

public interface IGameService
{
    /// <summary>Sends a game invite to any approved user (no match required — FR-27).</summary>
    Task<GameInvite> SendInviteAsync(string senderId, string receiverId, string gameType);

    Task<GameInvite?> GetInviteAsync(Guid inviteId);
    Task<IEnumerable<GameInvite>> GetPendingInvitesAsync(string userId);

    /// <summary>
    /// Accepts an invite and creates a GameSession.
    /// Returns the new session.
    /// </summary>
    Task<GameSession> AcceptInviteAsync(Guid inviteId, string acceptingUserId);

    /// <summary>Declines an invite — sets status to Declined.</summary>
    Task DeclineInviteAsync(Guid inviteId, string decliningUserId);

    Task<GameSession?> GetSessionAsync(Guid sessionId);
    Task<IEnumerable<GameSession>> GetActiveSessionsAsync(string userId);

    /// <summary>Persists the latest game state JSON.</summary>
    Task UpdateSessionStateAsync(Guid sessionId, string stateJson);

    Task EndSessionAsync(Guid sessionId, SessionStatus finalStatus);

    /// <summary>Returns true if both users share an active session (used by MessageService).</summary>
    Task<bool> ActiveSessionExistsAsync(string user1Id, string user2Id);
}
