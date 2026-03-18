using WebApp.Domain.Entities;

namespace WebApp.Domain.Interfaces;

public interface IModerationService
{
    Task<Block> BlockUserAsync(string blockerId, string blockedId);
    Task UnblockUserAsync(string blockerId, string blockedId);
    Task<bool> IsBlockedAsync(string user1Id, string user2Id);

    Task<Report> ReportUserAsync(string reporterId, string reportedId, string reason);
    Task<IEnumerable<Report>> GetPendingReportsAsync();
    Task ResolveReportAsync(Guid reportId, bool actionTaken);
}
