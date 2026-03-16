using Microsoft.AspNetCore.Identity;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Message : BaseEntity
    {
        /// <summary>Set when the message belongs to a match conversation.</summary>
        public Guid? ConversationId { get; set; }

        /// <summary>Set when the message belongs to an active game session.</summary>
        public Guid? GameSessionId { get; set; }

        public required string SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        public Conversation? Conversation { get; set; }
        public GameSession? GameSession { get; set; }
        public IdentityUser Sender { get; set; } = null!;
    }
}
