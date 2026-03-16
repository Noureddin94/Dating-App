using Microsoft.AspNetCore.Identity;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Like : BaseEntity
    {
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }

        /// <summary>true = like, false = dislike</summary>
        public bool IsLike { get; set; }

        public IdentityUser Sender { get; set; } = null!;
        public IdentityUser Receiver { get; set; } = null!;
    }
}
