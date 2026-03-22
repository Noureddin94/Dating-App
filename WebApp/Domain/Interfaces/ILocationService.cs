
using WebApp.Domain.ExternalModels;

namespace WebApp.Domain.Interfaces;

public interface ILocationService
{
    /// <summary>
    /// Converts a city name to GPS coordinates using the free
    /// Nominatim geocoding API (OpenStreetMap). No API key required.
    /// Rate limited to 1 request/second by OSM policy.
    /// </summary>
    Task<GeocodeResult?> GeocodeAsync(string city, string? country = null);

    /// <summary>
    /// Returns distance in kilometres between two profiles.
    /// Returns null if either profile has no coordinates stored.
    /// Pure Haversine calculation — no external API call.
    /// </summary>
    double? GetDistanceKm(
        double? lat1, double? lon1,
        double? lat2, double? lon2);

    /// <summary>
    /// Filters a list of profiles by maximum distance from origin coordinates.
    /// Profiles with no coordinates are included by default (opt-in filtering).
    /// </summary>
    IEnumerable<(T Profile, double? DistanceKm)> FilterByDistance<T>(
        IEnumerable<T> profiles,
        Func<T, double?> getLat,
        Func<T, double?> getLon,
        double originLat,
        double originLon,
        double? maxDistanceKm = null);
}
