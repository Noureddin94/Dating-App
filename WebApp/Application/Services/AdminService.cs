using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Domain.Entities;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class AdminService(AppDbContext context) : IAdminService
{
    public async Task<IEnumerable<UserProfile>> GetAllUsersAsync(
        string? nameFilter, AccountStatus? statusFilter)
    {
        var query = context.UserProfiles
            .Include(p => p.ProfileImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
            query = query.Where(p =>
                p.FirstName.Contains(nameFilter) ||
                p.LastName.Contains(nameFilter));

        if (statusFilter.HasValue)
            query = query.Where(p => p.Status == statusFilter.Value);

        return await query.OrderBy(p => p.LastName).ToListAsync();
    }

    public async Task ApproveUserAsync(string userId) =>
        await SetStatusAsync(userId, AccountStatus.Approved);

    public async Task RejectUserAsync(string userId) =>
        await SetStatusAsync(userId, AccountStatus.Rejected);

    public async Task SuspendUserAsync(string userId) =>
        await SetStatusAsync(userId, AccountStatus.Suspended);

    public async Task<UserProfile?> GetUserProfileAsync(string userId) =>
        await context.UserProfiles
            .Include(p => p.ProfileImages)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    private async Task SetStatusAsync(string userId, AccountStatus status)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException($"User profile not found for userId: {userId}");

        profile.Status = status;
        profile.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
