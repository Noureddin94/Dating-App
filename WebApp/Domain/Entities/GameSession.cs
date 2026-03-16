using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class GameSession : BaseEntity
    {
        public Guid InviteId { get; set; }
        public required string Player1Id { get; set; }
        public required string Player2Id { get; set; }
        public required string GameType { get; set; }

        /// <summary>Serialised game state (JSON). Format is game-type specific.</summary>
        public string StateJson { get; set; } = "{}";
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public GameInvite Invite { get; set; } = null!;
        public IdentityUser Player1 { get; set; } = null!;
        public IdentityUser Player2 { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = [];
    }
}
