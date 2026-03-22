using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Domain.Entities;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class ProfileService(AppDbContext context) : IProfileService
{
    private const int MaxImages = 6;

    public async Task<UserProfile?> GetByUserIdAsync(string userId) =>
        await context.UserProfiles
            .Include(p => p.ProfileImages)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<UserProfile> CreateAsync(
        string userId, string firstName, string lastName, DateOnly dateOfBirth)
    {
        var profile = new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Status = AccountStatus.Pending
        };
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
        var profile = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
        return profile?.Status ?? AccountStatus.Pending;
    }

    // ── Images ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProfileImage>> GetImagesAsync(string userId) =>
        await context.ProfileImages
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();

    public async Task<ProfileImage> AddImageAsync(string userId, string blobPath, bool isPrimary)
    {
        var count = await context.ProfileImages.CountAsync(i => i.UserId == userId);
        if (count >= MaxImages)
            throw new InvalidOperationException($"Maximum of {MaxImages} images allowed.");

        // If this is the first image or isPrimary requested, demote existing primary
        if (isPrimary)
        {
            var existing = await context.ProfileImages
                .Where(i => i.UserId == userId && i.IsPrimary)
                .ToListAsync();
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

        if (image.UserId != requestingUserId)
            throw new UnauthorizedAccessException();

        var all = await context.ProfileImages
            .Where(i => i.UserId == requestingUserId)
            .ToListAsync();

        all.ForEach(i => i.IsPrimary = i.Id == imageId);
        await context.SaveChangesAsync();
    }

    // ── Discovery feed ────────────────────────────────────────────────────────

    public async Task<IEnumerable<UserProfile>> GetDiscoveryFeedAsync(
    string? requestingUserId, int skip, int take)
    {
        var excluded = new HashSet<string>();

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
        }

        return await context.UserProfiles
            .Include(p => p.ProfileImages)
            .Where(p => p.Status == AccountStatus.Approved && !excluded.Contains(p.UserId))
            .OrderBy(_ => Guid.NewGuid())
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

}
