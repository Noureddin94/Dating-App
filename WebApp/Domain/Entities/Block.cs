using Microsoft.AspNetCore.Identity;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Block : BaseEntity
    {
        public required string BlockerId { get; set; }
        public required string BlockedId { get; set; }

        public IdentityUser Blocker { get; set; } = null!;
        public IdentityUser Blocked { get; set; } = null!;
    }
}
