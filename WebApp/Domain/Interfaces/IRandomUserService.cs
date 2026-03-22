using WebApp.Domain.ExternalModels;

namespace WebApp.Domain.Interfaces;

public interface IRandomUserService
{
    /// <summary>
    /// Fetches randomly generated user profiles from randomuser.me.
    /// Returns cleaned GeneratedUser objects ready to store in the database.
    /// </summary>
    Task<List<GeneratedUser>> GetRandomUsersAsync(
        int count = 10,
        string? gender = null,
        string? nationality = null);

    /// <summary>
    /// Fetches a single random user.
    /// </summary>
    Task<GeneratedUser?> GetSingleRandomUserAsync(
        string? gender = null,
        string? nationality = null);
}
