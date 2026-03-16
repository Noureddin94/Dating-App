using Microsoft.AspNetCore.Identity;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Match : BaseEntity
    {
        public required string User1Id { get; set; }
        public required string User2Id { get; set; }
        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

        public IdentityUser User1 { get; set; } = null!;
        public IdentityUser User2 { get; set; } = null!;
        public Conversation? Conversation { get; set; }
    }
}
