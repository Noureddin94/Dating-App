using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class LikeService(AppDbContext context) : ILikeService
{
    private const int DailyLikeLimit = 20; // FR-19

    public async Task<Like> SendAsync(string senderId, string receiverId, bool isLike)
    {
        // Prevent duplicate action on same profile (US-02)
        var existing = await context.Likes
            .FirstOrDefaultAsync(l => l.SenderId == senderId && l.ReceiverId == receiverId);
        if (existing is not null)
            throw new InvalidOperationException("You have already acted on this profile.");

        // Enforce daily like limit only for actual likes, not dislikes (FR-19)
        if (isLike)
        {
            var count = await GetDailyLikeCountAsync(senderId);
            if (count >= DailyLikeLimit)
                throw new InvalidOperationException(
                    $"Daily like limit of {DailyLikeLimit} reached. Try again tomorrow.");

            await IncrementDailyCountAsync(senderId, ActionType.Like);
        }

        var like = new Like
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            IsLike = isLike
        };

        context.Likes.Add(like);
        await context.SaveChangesAsync();
        return like;
    }

    public async Task<int> GetDailyLikeCountAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await context.DailyActionCounts
            .FirstOrDefaultAsync(d =>
                d.UserId == userId &&
                d.ActionType == ActionType.Like &&
                d.Date == today);
        return record?.Count ?? 0;
    }

    public async Task<bool> HasLikedAsync(string senderId, string receiverId) =>
        await context.Likes.AnyAsync(l =>
            l.SenderId == senderId &&
            l.ReceiverId == receiverId &&
            l.IsLike);

    public async Task<bool> IsMutualLikeAsync(string user1Id, string user2Id) =>
        await context.Likes.AnyAsync(l =>
            l.SenderId == user1Id && l.ReceiverId == user2Id && l.IsLike) &&
        await context.Likes.AnyAsync(l =>
            l.SenderId == user2Id && l.ReceiverId == user1Id && l.IsLike);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task IncrementDailyCountAsync(string userId, ActionType actionType)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await context.DailyActionCounts
            .FirstOrDefaultAsync(d =>
                d.UserId == userId &&
                d.ActionType == actionType &&
                d.Date == today);

        if (record is null)
        {
            context.DailyActionCounts.Add(new DailyActionCount
            {
                UserId = userId,
                ActionType = actionType,
                Date = today,
                Count = 1
            });
        }
        else
        {
            record.Count++;
        }

        // SaveChanges is called by the calling method after adding the Like entity
    }
}
