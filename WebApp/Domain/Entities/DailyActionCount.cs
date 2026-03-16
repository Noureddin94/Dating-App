using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities


/// <summary>
/// Tracks daily rate-limited actions per user (FR-19: 20 likes/day, FR-25: 5 unmatched messages/day).
/// One row per user + action type + calendar date.
/// </summary>
{
    public class DailyActionCount : BaseEntity
    {
        public required string UserId { get; set; }
        public ActionType ActionType { get; set; }
        public DateOnly Date { get; set; }
        public int Count { get; set; }

        public IdentityUser User { get; set; } = null!;
    }
}
