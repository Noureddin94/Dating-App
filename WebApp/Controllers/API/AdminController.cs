using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize(Policy = "AdminOnly")]
public class AdminController(
    IAdminService adminService,
    IModerationService moderationService) : BaseApiController
{
    // GET api/admin/users?nameFilter=john&statusFilter=Pending
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? nameFilter,
        [FromQuery] AccountStatus? statusFilter)
    {
        try
        {
            var users = await adminService.GetAllUsersAsync(nameFilter, statusFilter);
            return Ok(users.Select(p => new AdminUserResponse(
                p.UserId,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.Status,
                p.City,
                p.ProfileImages.Count)));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/admin/users/{userId}
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        try
        {
            var profile = await adminService.GetUserProfileAsync(userId);
            if (profile is null) return NotFound(new { error = "User not found." });

            return Ok(new AdminUserResponse(
                profile.UserId,
                profile.FirstName,
                profile.LastName,
                profile.DateOfBirth,
                profile.Status,
                profile.City,
                profile.ProfileImages.Count));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/admin/users/{userId}/approve
    [HttpPost("users/{userId}/approve")]
    public async Task<IActionResult> ApproveUser(string userId)
    {
        try
        {
            await adminService.ApproveUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/admin/users/{userId}/reject
    [HttpPost("users/{userId}/reject")]
    public async Task<IActionResult> RejectUser(string userId)
    {
        try
        {
            await adminService.RejectUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/admin/users/{userId}/suspend
    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(string userId)
    {
        try
        {
            await adminService.SuspendUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/admin/reports
    [HttpGet("reports")]
    public async Task<IActionResult> GetPendingReports()
    {
        try
        {
            var reports = await moderationService.GetPendingReportsAsync();
            return Ok(reports.Select(r => new
            {
                r.Id,
                r.ReporterId,
                r.ReportedId,
                r.Reason,
                r.Status,
                r.CreatedAt
            }));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/admin/reports/{reportId}/resolve
    [HttpPost("reports/{reportId:guid}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid reportId, [FromQuery] bool actionTaken)
    {
        try
        {
            await moderationService.ResolveReportAsync(reportId, actionTaken);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
