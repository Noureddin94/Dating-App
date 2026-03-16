using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class GameInvite : BaseEntity
    {
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public required string GameType { get; set; }
        public InviteStatus Status { get; set; } = InviteStatus.Pending;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

        public IdentityUser Sender { get; set; } = null!;
        public IdentityUser Receiver { get; set; } = null!;
        public GameSession? GameSession { get; set; }
    }
}
