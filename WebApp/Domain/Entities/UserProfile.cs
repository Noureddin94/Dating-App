using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;

namespace WebApp.Infrastructure.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public required string UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public string? Location { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Pending;

        public IdentityUser User { get; set; } = null!;
        public ICollection<ProfileImage> ProfileImages { get; set; } = [];
    }
}
