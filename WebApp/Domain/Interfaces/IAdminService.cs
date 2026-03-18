using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserProfile>> GetAllUsersAsync(string? nameFilter, AccountStatus? statusFilter);
    Task ApproveUserAsync(string userId);
    Task RejectUserAsync(string userId);
    Task SuspendUserAsync(string userId);
    Task<UserProfile?> GetUserProfileAsync(string userId);
}
