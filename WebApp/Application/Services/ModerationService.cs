using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Application.Services;

public class ModerationService(AppDbContext context) : IModerationService
{
    public async Task<Block> BlockUserAsync(string blockerId, string blockedId)
    {
        var exists = await context.Blocks
            .AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

        if (exists)
            throw new InvalidOperationException("User is already blocked.");

        var block = new Block { BlockerId = blockerId, BlockedId = blockedId };
        context.Blocks.Add(block);
        await context.SaveChangesAsync();
        return block;
    }

    public async Task UnblockUserAsync(string blockerId, string blockedId)
    {
        var block = await context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId)
            ?? throw new KeyNotFoundException("Block not found.");

        context.Blocks.Remove(block);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsBlockedAsync(string user1Id, string user2Id) =>
        await context.Blocks.AnyAsync(b =>
            (b.BlockerId == user1Id && b.BlockedId == user2Id) ||
            (b.BlockerId == user2Id && b.BlockedId == user1Id));

    public async Task<Report> ReportUserAsync(string reporterId, string reportedId, string reason)
    {
        var report = new Report
        {
            ReporterId = reporterId,
            ReportedId = reportedId,
            Reason = reason,
            Status = ReportStatus.Pending
        };
        context.Reports.Add(report);
        await context.SaveChangesAsync();
        return report;
    }

    public async Task<IEnumerable<Report>> GetPendingReportsAsync() =>
        await context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Reported)
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task ResolveReportAsync(Guid reportId, bool actionTaken)
    {
        var report = await context.Reports.FindAsync(reportId)
            ?? throw new KeyNotFoundException("Report not found.");

        report.Status = actionTaken ? ReportStatus.ActionTaken : ReportStatus.Dismissed;
        report.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
