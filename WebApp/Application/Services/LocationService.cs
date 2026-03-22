using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WebApp.Domain.ExternalModels;
using WebApp.Domain.Interfaces;

namespace WebApp.Application.Services;

public class LocationService(
    HttpClient httpClient,
    ILogger<LocationService> logger) : ILocationService
{
    public async Task<GeocodeResult?> GeocodeAsync(string city, string? country = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return null;

        var query = country is not null
            ? $"{Uri.EscapeDataString(city)},{Uri.EscapeDataString(country)}"
            : Uri.EscapeDataString(city);

        var url = $"search?q={query}&format=json&limit=1&addressdetails=1";

        try
        {
            var results = await httpClient
                .GetFromJsonAsync<List<NominatimResult>>(url);

            var first = results?.FirstOrDefault();
            if (first is null)
            {
                logger.LogWarning("Nominatim returned no results for city: {City}", city);
                return null;
            }

            var cityName = first.Address?.City
                        ?? first.Address?.Town
                        ?? first.Address?.Village
                        ?? city;

            var countryName = first.Address?.Country ?? string.Empty;

            return new GeocodeResult(
                cityName,
                countryName,
                double.Parse(first.Lat, System.Globalization.CultureInfo.InvariantCulture),
                double.Parse(first.Lon, System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Nominatim geocoding request failed for city: {City}", city);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during geocoding for city: {City}", city);
            return null;
        }
    }

    public double? GetDistanceKm(
        double? lat1, double? lon1,
        double? lat2, double? lon2)
    {
        if (lat1 is null || lon1 is null || lat2 is null || lon2 is null)
            return null;

        return DistanceCalculator.GetDistanceKm(
            lat1.Value, lon1.Value,
            lat2.Value, lon2.Value);
    }

    public IEnumerable<(T Profile, double? DistanceKm)> FilterByDistance<T>(
        IEnumerable<T> profiles,
        Func<T, double?> getLat,
        Func<T, double?> getLon,
        double originLat,
        double originLon,
        double? maxDistanceKm = null)
    {
        foreach (var profile in profiles)
        {
            var lat = getLat(profile);
            var lon = getLon(profile);

            double? distance = (lat.HasValue && lon.HasValue)
                ? DistanceCalculator.GetDistanceKm(
                    originLat, originLon,
                    lat.Value, lon.Value)
                : null;

            // Include profiles with no coordinates (no location set yet)
            if (maxDistanceKm is null || distance is null || distance <= maxDistanceKm)
                yield return (profile, distance);
        }
    }

    // ── Nominatim response models (internal) ──────────────────────────────────

    private class NominatimResult
    {
        [JsonPropertyName("lat")]   public string Lat { get; set; } = "";
        [JsonPropertyName("lon")]   public string Lon { get; set; } = "";
        [JsonPropertyName("address")] public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        [JsonPropertyName("city")]    public string? City    { get; set; }
        [JsonPropertyName("town")]    public string? Town    { get; set; }
        [JsonPropertyName("village")] public string? Village { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
    }
}
