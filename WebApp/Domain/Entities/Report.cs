using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Domain.Entities
{
    public class Report : BaseEntity
    {
        public required string ReporterId { get; set; }
        public required string ReportedId { get; set; }
        public required string Reason { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public IdentityUser Reporter { get; set; } = null!;
        public IdentityUser Reported { get; set; } = null!;
    }
}
