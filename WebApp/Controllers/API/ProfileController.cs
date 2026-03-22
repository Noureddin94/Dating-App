using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Entities;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class ProfileController(IProfileService profileService) : BaseApiController
{
    // GET api/profile/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var profile = await profileService.GetByUserIdAsync(CurrentUserId);
            if (profile is null)
                return NotFound(new { error = "Profile not found. Please complete registration." });

            return Ok(MapToResponse(profile));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/profile/{userId}
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(string userId)
    {
        try
        {
            var profile = await profileService.GetByUserIdAsync(userId);
            if (profile is null) return NotFound(new { error = "Profile not found." });
            return Ok(MapToResponse(profile));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/profile
    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
    {
        try
        {
            var existing = await profileService.GetByUserIdAsync(CurrentUserId);
            if (existing is not null)
                return Conflict(new { error = "Profile already exists." });

            var profile = await profileService.CreateAsync(
                CurrentUserId,
                request.FirstName,
                request.LastName,
                request.DateOfBirth);

            if (request.Bio is not null)       profile.Bio = request.Bio;
            if (request.Gender is not null)    profile.Gender = request.Gender;
            if (request.Location is not null)  profile.Location = request.Location;

            await profileService.UpdateAsync(profile);

            return CreatedAtAction(nameof(GetMyProfile), MapToResponse(profile));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // PUT api/profile/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var profile = await profileService.GetByUserIdAsync(CurrentUserId)
                ?? throw new KeyNotFoundException("Profile not found.");

            if (request.FirstName is not null)  profile.FirstName = request.FirstName;
            if (request.LastName is not null)   profile.LastName = request.LastName;
            if (request.Bio is not null)        profile.Bio = request.Bio;
            if (request.Gender is not null)     profile.Gender = request.Gender;
            if (request.Location is not null)   profile.Location = request.Location;

            await profileService.UpdateAsync(profile);
            return Ok(MapToResponse(profile));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // POST api/profile/images
    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImage([FromForm] UploadImageRequest request)
    {
        try
        {
            if (request.File.Length == 0)
                return BadRequest(new { error = "No file provided." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(request.File.ContentType))
                return BadRequest(new { error = "Only JPEG, PNG and WebP images are allowed." });

            var blobPath = $"profiles/{CurrentUserId}/{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";

            var image = await profileService.AddImageAsync(CurrentUserId, blobPath, request.IsPrimary);
            return Ok(new ProfileImageResponse(image.Id, image.BlobPath, image.IsPrimary, image.SortOrder));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // DELETE api/profile/images/{imageId}
    [HttpDelete("images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        try
        {
            await profileService.DeleteImageAsync(imageId, CurrentUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // PATCH api/profile/images/{imageId}/primary
    [HttpPatch("images/{imageId:guid}/primary")]
    public async Task<IActionResult> SetPrimaryImage(Guid imageId)
    {
        try
        {
            await profileService.SetPrimaryImageAsync(imageId, CurrentUserId);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/profile/discovery?skip=0&take=10
    [HttpGet("discovery")]
    public async Task<IActionResult> GetDiscoveryFeed([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            var profiles = await profileService.GetDiscoveryFeedAsync(CurrentUserId, skip, take);
            return Ok(profiles.Select(MapToResponse));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/profile/guest/discovery?skip=0&take=10
    // No [Authorize] — guests can call this freely
    [AllowAnonymous]
    [HttpGet("guest/discovery")]
    public async Task<IActionResult> GetGuestDiscoveryFeed(
        [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            // Pass null as userId — ProfileService returns approved profiles
            // but has no user context to exclude acted-on profiles
            var profiles = await profileService.GetDiscoveryFeedAsync(null!, skip, take);
            return Ok(profiles.Select(MapToResponse));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static ProfileResponse MapToResponse(UserProfile p) => new(
        p.Id,
        p.UserId,
        p.FirstName,
        p.LastName,
        p.DateOfBirth,
        p.Bio,
        p.Gender,
        p.Location,
        p.Status,
        p.ProfileImages.Select(i =>
            new ProfileImageResponse(i.Id, i.BlobPath, i.IsPrimary, i.SortOrder)).ToList());
}
