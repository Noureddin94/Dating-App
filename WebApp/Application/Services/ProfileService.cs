using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Domain.Entities;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class ProfileService(
    AppDbContext context,
    ILocationService locationService) : IProfileService
{
    private const int MaxImages = 6;

    public async Task<UserProfile?> GetByUserIdAsync(string userId) =>
        await context.UserProfiles
            .Include(p => p.ProfileImages)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<UserProfile> CreateAsync(
        string userId, string firstName, string lastName,
        DateOnly dateOfBirth, string? city = null, string? country = null)
    {
        var profile = new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Status = AccountStatus.Pending
        };
        if (!string.IsNullOrWhiteSpace(city))
            await ApplyLocationAsync(profile, city, country);
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        context.UserProfiles.Update(profile);
        await context.SaveChangesAsync();
    }

    public async Task<AccountStatus> GetStatusAsync(string userId)
    {
        var profile = await context.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
        return profile?.Status ?? AccountStatus.Pending;
    }

    public async Task ApplyLocationAsync(UserProfile profile, string city, string? country)
    {
        var result = await locationService.GeocodeAsync(city, country);
        if (result is null) return;
        profile.City = result.City; profile.Country = result.Country;
        profile.Latitude = result.Latitude; profile.Longitude = result.Longitude;
    }

    public async Task<IEnumerable<ProfileImage>> GetImagesAsync(string userId) =>
        await context.ProfileImages.Where(i => i.UserId == userId)
            .OrderBy(i => i.SortOrder).ToListAsync();

    public async Task<ProfileImage> AddImageAsync(string userId, string blobPath, bool isPrimary)
    {
        var count = await context.ProfileImages.CountAsync(i => i.UserId == userId);
        if (count >= MaxImages)
            throw new InvalidOperationException($"Maximum of {MaxImages} images allowed.");
        if (isPrimary)
        {
            var existing = await context.ProfileImages
                .Where(i => i.UserId == userId && i.IsPrimary).ToListAsync();
            existing.ForEach(i => i.IsPrimary = false);
        }
        var image = new ProfileImage
        {
            UserId = userId,
            BlobPath = blobPath,
            IsPrimary = isPrimary || count == 0,
            SortOrder = count
        };
        context.ProfileImages.Add(image);
        await context.SaveChangesAsync();
        return image;
    }

    public async Task DeleteImageAsync(Guid imageId, string requestingUserId)
    {
        var image = await context.ProfileImages.FindAsync(imageId)
            ?? throw new KeyNotFoundException("Image not found.");
        if (image.UserId != requestingUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's image.");
        context.ProfileImages.Remove(image);
        await context.SaveChangesAsync();
    }

    public async Task SetPrimaryImageAsync(Guid imageId, string requestingUserId)
    {
        var image = await context.ProfileImages.FindAsync(imageId)
            ?? throw new KeyNotFoundException("Image not found.");
        if (image.UserId != requestingUserId) throw new UnauthorizedAccessException();
        var all = await context.ProfileImages
            .Where(i => i.UserId == requestingUserId).ToListAsync();
        all.ForEach(i => i.IsPrimary = i.Id == imageId);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<(UserProfile Profile, double? DistanceKm)>>
        GetDiscoveryFeedAsync(string? requestingUserId, int skip, int take,
            double? maxDistanceKm = null, string? cityFilter = null)
    {
        var excluded = new HashSet<string>();
        double? originLat = null, originLon = null;

        if (requestingUserId is not null)
        {
            var actedOn = await context.Likes
                .Where(l => l.SenderId == requestingUserId)
                .Select(l => l.ReceiverId).ToListAsync();
            var blocked = await context.Blocks
                .Where(b => b.BlockerId == requestingUserId || b.BlockedId == requestingUserId)
                .Select(b => b.BlockerId == requestingUserId ? b.BlockedId : b.BlockerId)
                .ToListAsync();
            excluded = actedOn.Union(blocked).Append(requestingUserId).ToHashSet();
            var requester = await context.UserProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == requestingUserId);
            originLat = requester?.Latitude;
            originLon = requester?.Longitude;
        }

        var query = context.UserProfiles.AsNoTracking()
            .Where(p => p.Status == AccountStatus.Approved && !excluded.Contains(p.UserId));

        if (!string.IsNullOrWhiteSpace(cityFilter))
            query = query.Where(p => p.City != null &&
                p.City.ToLower().Contains(cityFilter.ToLower()));

        var profiles = await query.ToListAsync();

        IEnumerable<(UserProfile, double?)> results = originLat.HasValue && originLon.HasValue
            ? locationService.FilterByDistance(profiles,
                p => p.Latitude, p => p.Longitude,
                originLat.Value, originLon.Value, maxDistanceKm)
            : profiles.Select(p => (p, (double?)null));

        return results
            .OrderBy(x => x.Item2 ?? double.MaxValue)
            .Skip(skip).Take(take).ToList();
    }
}
