using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class MatchService(AppDbContext context, ILikeService likeService) : IMatchService
{
    public async Task<Match?> TryCreateMatchAsync(string user1Id, string user2Id)
    {
        // No duplicate matches (FR-22)
        if (await IsMatchedAsync(user1Id, user2Id))
            return null;

        if (!await likeService.IsMutualLikeAsync(user1Id, user2Id))
            return null;

        // Always store with the lexicographically smaller ID as User1 to
        // guarantee the unique index on (User1Id, User2Id) works correctly
        var (a, b) = string.Compare(user1Id, user2Id) < 0
            ? (user1Id, user2Id)
            : (user2Id, user1Id);

        var match = new Match
        {
            User1Id = a,
            User2Id = b,
            MatchedAt = DateTime.UtcNow,
            Conversation = new Conversation()  // auto-create conversation (FR-24)
        };

        context.Matches.Add(match);
        await context.SaveChangesAsync();
        return match;
    }

    public async Task<IEnumerable<Match>> GetMatchesForUserAsync(string userId) =>
        await context.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Include(m => m.Conversation)
            .Where(m => m.User1Id == userId || m.User2Id == userId)
            .OrderByDescending(m => m.MatchedAt)
            .ToListAsync();

    public async Task<Match?> GetMatchAsync(string user1Id, string user2Id)
    {
        var (a, b) = string.Compare(user1Id, user2Id) < 0
            ? (user1Id, user2Id)
            : (user2Id, user1Id);

        return await context.Matches
            .Include(m => m.Conversation)
            .FirstOrDefaultAsync(m => m.User1Id == a && m.User2Id == b);
    }

    public async Task<bool> IsMatchedAsync(string user1Id, string user2Id)
    {
        var (a, b) = string.Compare(user1Id, user2Id) < 0
            ? (user1Id, user2Id)
            : (user2Id, user1Id);

        return await context.Matches
            .AnyAsync(m => m.User1Id == a && m.User2Id == b);
    }
}
