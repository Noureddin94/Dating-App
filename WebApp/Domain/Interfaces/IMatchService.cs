using WebApp.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface IMatchService
{
    /// <summary>
    /// Checks for a mutual like and creates a Match + Conversation if found.
    /// Returns the new Match or null if no mutual like exists.
    /// </summary>
    Task<Match?> TryCreateMatchAsync(string user1Id, string user2Id);

    Task<IEnumerable<Match>> GetMatchesForUserAsync(string userId);
    Task<Match?> GetMatchAsync(string user1Id, string user2Id);
    Task<bool> IsMatchedAsync(string user1Id, string user2Id);
}
