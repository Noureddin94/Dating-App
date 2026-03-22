namespace WebApp.Domain.ExternalModels;

public record Coordinates(double Latitude, double Longitude);

public record GeocodeResult(
    string City,
    string Country,
    double Latitude,
    double Longitude);

/// <summary>
/// Haversine formula — calculates great-circle distance between two
/// GPS coordinates. No external API needed for this calculation.
/// Accurate to within ~0.5% for typical dating-app distances.
/// </summary>
public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;

    public static double GetDistanceKm(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    public static double GetDistanceMiles(
        double lat1, double lon1,
        double lat2, double lon2) =>
        GetDistanceKm(lat1, lon1, lat2, lon2) * 0.621371;

    private static double ToRad(double degrees) =>
        degrees * Math.PI / 180.0;
}
