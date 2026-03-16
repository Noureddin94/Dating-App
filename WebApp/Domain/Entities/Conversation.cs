using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public Guid MatchId { get; set; }

        public Match Match { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = [];
    }
}
