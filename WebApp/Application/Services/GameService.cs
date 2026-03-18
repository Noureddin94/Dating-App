using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class GameService(AppDbContext context) : IGameService
{
    public async Task<GameInvite> SendInviteAsync(string senderId, string receiverId, string gameType)
    {
        // Prevent spamming — one pending invite per pair per game type
        var pendingExists = await context.GameInvites.AnyAsync(i =>
            i.SenderId == senderId &&
            i.ReceiverId == receiverId &&
            i.GameType == gameType &&
            i.Status == InviteStatus.Pending &&
            i.ExpiresAt > DateTime.UtcNow);

        if (pendingExists)
            throw new InvalidOperationException(
                "A pending invite already exists for this game type.");

        var invite = new GameInvite
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            GameType = gameType,
            Status = InviteStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        context.GameInvites.Add(invite);
        await context.SaveChangesAsync();
        return invite;
    }

    public async Task<GameInvite?> GetInviteAsync(Guid inviteId) =>
        await context.GameInvites
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .FirstOrDefaultAsync(i => i.Id == inviteId);

    public async Task<IEnumerable<GameInvite>> GetPendingInvitesAsync(string userId) =>
        await context.GameInvites
            .Include(i => i.Sender)
            .Where(i =>
                i.ReceiverId == userId &&
                i.Status == InviteStatus.Pending &&
                i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

    public async Task<GameSession> AcceptInviteAsync(Guid inviteId, string acceptingUserId)
    {
        var invite = await context.GameInvites.FindAsync(inviteId)
            ?? throw new KeyNotFoundException("Invite not found.");

        if (invite.ReceiverId != acceptingUserId)
            throw new UnauthorizedAccessException("This invite is not for you.");

        if (invite.Status != InviteStatus.Pending)
            throw new InvalidOperationException("Invite is no longer pending.");

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            invite.Status = InviteStatus.Expired;
            await context.SaveChangesAsync();
            throw new InvalidOperationException("Invite has expired.");
        }

        invite.Status = InviteStatus.Accepted;

        var session = new GameSession
        {
            InviteId = invite.Id,
            Player1Id = invite.SenderId,
            Player2Id = invite.ReceiverId,
            GameType = invite.GameType,
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow
        };

        context.GameSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task DeclineInviteAsync(Guid inviteId, string decliningUserId)
    {
        var invite = await context.GameInvites.FindAsync(inviteId)
            ?? throw new KeyNotFoundException("Invite not found.");

        if (invite.ReceiverId != decliningUserId)
            throw new UnauthorizedAccessException("This invite is not for you.");

        invite.Status = InviteStatus.Declined;
        await context.SaveChangesAsync();
    }

    public async Task<GameSession?> GetSessionAsync(Guid sessionId) =>
        await context.GameSessions
            .Include(s => s.Player1)
            .Include(s => s.Player2)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

    public async Task<IEnumerable<GameSession>> GetActiveSessionsAsync(string userId) =>
        await context.GameSessions
            .Include(s => s.Player1)
            .Include(s => s.Player2)
            .Where(s =>
                (s.Player1Id == userId || s.Player2Id == userId) &&
                s.Status == SessionStatus.Active)
            .ToListAsync();

    public async Task UpdateSessionStateAsync(Guid sessionId, string stateJson)
    {
        var session = await context.GameSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session not found.");

        session.StateJson = stateJson;
        session.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task EndSessionAsync(Guid sessionId, SessionStatus finalStatus)
    {
        var session = await context.GameSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session not found.");

        session.Status = finalStatus;
        session.EndedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<bool> ActiveSessionExistsAsync(string user1Id, string user2Id) =>
        await context.GameSessions.AnyAsync(s =>
            s.Status == SessionStatus.Active &&
            ((s.Player1Id == user1Id && s.Player2Id == user2Id) ||
             (s.Player1Id == user2Id && s.Player2Id == user1Id)));
}
