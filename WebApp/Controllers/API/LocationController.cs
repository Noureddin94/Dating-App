using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;
using WebApp.Presentation.DTOs;

namespace WebApp.Controllers.API;

[Authorize]
public class LocationController(
    ILocationService locationService,
    IProfileService profileService) : BaseApiController
{
    // PUT api/location
    // Updates the current user's location by geocoding the provided city name
    [HttpPut]
    public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
    {
        try
        {
            var profile = await profileService.GetByUserIdAsync(CurrentUserId)
                ?? throw new KeyNotFoundException("Profile not found.");

            var result = await locationService.GeocodeAsync(request.City, request.Country);
            if (result is null)
                return BadRequest(new { error = $"Could not find coordinates for '{request.City}'. Try a different city name." });

            profile.City      = result.City;
            profile.Country   = result.Country;
            profile.Latitude  = result.Latitude;
            profile.Longitude = result.Longitude;

            await profileService.UpdateAsync(profile);

            return Ok(new LocationUpdateResponse(
                result.City,
                result.Country,
                result.Latitude,
                result.Longitude));
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/location/distance/{userId}
    // Returns the distance in km between the current user and another user
    [HttpGet("distance/{userId}")]
    public async Task<IActionResult> GetDistance(string userId)
    {
        try
        {
            var myProfile    = await profileService.GetByUserIdAsync(CurrentUserId);
            var otherProfile = await profileService.GetByUserIdAsync(userId);

            if (myProfile is null || otherProfile is null)
                return NotFound(new { error = "One or both profiles not found." });

            var distanceKm = locationService.GetDistanceKm(
                myProfile.Latitude,  myProfile.Longitude,
                otherProfile.Latitude, otherProfile.Longitude);

            if (distanceKm is null)
                return Ok(new
                {
                    distanceKm     = (double?)null,
                    distanceMiles  = (double?)null,
                    message        = "Distance unavailable — one or both users have not set their location."
                });

            return Ok(new
            {
                distanceKm    = Math.Round(distanceKm.Value, 1),
                distanceMiles = Math.Round(distanceKm.Value * 0.621371, 1),
                fromCity      = myProfile.City,
                toCity        = otherProfile.City
            });
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/location/nearby?maxDistanceKm=50&skip=0&take=10
    // Discovery feed filtered to users within a maximum distance
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyUsers(
        [FromQuery] double maxDistanceKm = 50,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        try
        {
            if (maxDistanceKm <= 0 || maxDistanceKm > 20000)
                return BadRequest(new { error = "maxDistanceKm must be between 1 and 20000." });

            var myProfile = await profileService.GetByUserIdAsync(CurrentUserId);
            if (myProfile?.Latitude is null)
                return BadRequest(new { error = "Please set your location first via PUT /api/location." });

            var results = await profileService.GetDiscoveryFeedAsync(
                CurrentUserId, skip, take, maxDistanceKm);

            var images = new Dictionary<string, List<ProfileImageResponse>>();
            foreach (var (profile, _) in results)
            {
                var profileImages = await profileService.GetImagesAsync(profile.UserId);
                images[profile.UserId] = profileImages
                    .Select(i => new ProfileImageResponse(i.Id, i.BlobPath, i.IsPrimary, i.SortOrder))
                    .ToList();
            }

            var response = results.Select(x =>
            {
                var age = DateTime.Today.Year - x.Profile.DateOfBirth.Year;
                return new DiscoveryProfileResponse(
                    x.Profile.Id,
                    x.Profile.UserId,
                    x.Profile.FirstName,
                    age,
                    x.Profile.Bio,
                    x.Profile.Gender,
                    x.Profile.City,
                    x.Profile.Country,
                    x.DistanceKm.HasValue ? Math.Round(x.DistanceKm.Value, 1) : null,
                    images.GetValueOrDefault(x.Profile.UserId, []));
            });

            return Ok(response);
        }
        catch (Exception ex) { return HandleException(ex); }
    }
}
