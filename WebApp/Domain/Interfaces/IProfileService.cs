using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface IProfileService
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile> CreateAsync(string userId, string firstName, string lastName, DateOnly dateOfBirth, string? city = null, string? country =null);
    Task UpdateAsync(UserProfile profile);
    Task<AccountStatus> GetStatusAsync(string userId);

    // Images (max 6 per FR-15)
    Task<IEnumerable<ProfileImage>> GetImagesAsync(string userId);
    Task<ProfileImage> AddImageAsync(string userId, string blobPath, bool isPrimary);
    Task DeleteImageAsync(Guid imageId, string requestingUserId);
    Task SetPrimaryImageAsync(Guid imageId, string requestingUserId);

    // Discovery feed — approved users only, excluding blocks/dislikes (FR-17)
    Task<IEnumerable<(UserProfile Profile, double? DistanceKm)>> GetDiscoveryFeedAsync(
        string? requestingUserId, 
        int skip, 
        int take, 
        double? maxDistanceKm = null, 
        string? cityFilter = null);
}
