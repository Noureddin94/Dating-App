using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public required string UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        /// <summary>GPS latitude — populated via geocoding when user sets their city.</summary>
        public double? Latitude { get; set; }

        /// <summary>GPS longitude — populated via geocoding when user sets their city.</summary>
        public double? Longitude { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Pending;

        public IdentityUser User { get; set; } = null!;
        public ICollection<ProfileImage> ProfileImages { get; set; } = [];
    }
}
