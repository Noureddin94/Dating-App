using Microsoft.AspNetCore.Identity;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class ProfileImage : BaseEntity
    {
        public required string UserId { get; set; }
        public required string BlobPath { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }

        public IdentityUser User { get; set; } = null!;
    }
}
