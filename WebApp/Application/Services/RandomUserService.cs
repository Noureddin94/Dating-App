using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Domain.ExternalModels;
using WebApp.Domain.Interfaces;

namespace WebApp.Application.Services;

public class RandomUserService(
    HttpClient httpClient,
    ILogger<RandomUserService> logger) : IRandomUserService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<List<GeneratedUser>> GetRandomUsersAsync(
        int count = 10,
        string? gender = null,
        string? nationality = null)
    {
        var url = BuildUrl(count, gender, nationality);

        try
        {
            logger.LogInformation(
                "Fetching {Count} random users from randomuser.me", count);

            var response = await httpClient
                .GetFromJsonAsync<RandomUserApiResponse>(url, JsonOptions);

            if (response?.Results is null || response.Results.Count == 0)
            {
                logger.LogWarning("randomuser.me returned empty results for URL: {Url}", url);
                return [];
            }

            logger.LogInformation(
                "Successfully fetched {Count} users from randomuser.me (seed: {Seed})",
                response.Results.Count, response.Info.Seed);

            return response.Results
                .Select(MapToGeneratedUser)
                .Where(u => u is not null)
                .Cast<GeneratedUser>()
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to reach randomuser.me API");
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching random users");
            return [];
        }
    }

    public async Task<GeneratedUser?> GetSingleRandomUserAsync(
        string? gender = null,
        string? nationality = null)
    {
        var users = await GetRandomUsersAsync(1, gender, nationality);
        return users.FirstOrDefault();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildUrl(int count, string? gender, string? nationality)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["results"]  = count.ToString();
        query["inc"]      = "name,gender,location,email,login,dob,phone,picture,nat";

        if (!string.IsNullOrWhiteSpace(gender))
            query["gender"] = gender.ToLower();

        if (!string.IsNullOrWhiteSpace(nationality))
            query["nat"] = nationality.ToUpper();

        return $"api/?{query}";
    }

    private static GeneratedUser? MapToGeneratedUser(RandomUserResult result)
    {
        try
        {
            // Parse coordinates — API returns strings
            if (!double.TryParse(
                    result.Location.Coordinates.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var lat))
                lat = 0;

            if (!double.TryParse(
                    result.Location.Coordinates.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var lon))
                lon = 0;

            // Parse date of birth
            var dob = DateTime.TryParse(
                result.Dob.Date,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDob)
                ? DateOnly.FromDateTime(parsedDob)
                : DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-result.Dob.Age));

            // Capitalise gender
            var gender = result.Gender.Length > 0
                ? char.ToUpper(result.Gender[0]) + result.Gender[1..]
                : result.Gender;

            return new GeneratedUser(
                FirstName:           result.Name.First,
                LastName:            result.Name.Last,
                Email:               result.Email,
                Gender:              gender,
                Age:                 result.Dob.Age,
                DateOfBirth:         dob,
                City:                result.Location.City,
                Country:             result.Location.Country,
                Latitude:            lat,
                Longitude:           lon,
                PictureUrl:          result.Picture.Large,
                PictureThumbnailUrl: result.Picture.Thumbnail);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
