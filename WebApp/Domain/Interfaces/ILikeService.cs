using WebApp.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface ILikeService
{
    /// <summary>
    /// Records a like or dislike. Returns the created Like.
    /// Throws InvalidOperationException if daily limit reached (FR-19).
    /// </summary>
    Task<Like> SendAsync(string senderId, string receiverId, bool isLike);

    Task<int> GetDailyLikeCountAsync(string userId);
    Task<bool> HasLikedAsync(string senderId, string receiverId);

    /// <summary>Returns true if a mutual like exists (used by MatchService).</summary>
    Task<bool> IsMutualLikeAsync(string user1Id, string user2Id);
}
