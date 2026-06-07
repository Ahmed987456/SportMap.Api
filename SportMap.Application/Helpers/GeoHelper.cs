namespace SportMap.Application.Helpers;

public static class GeoHelper
{
    // Haversine Formula
    // بتحسب المسافة بين نقطتين على سطح الأرض بالكيلومتر
    public static double CalculateDistanceInKm(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        // نصف قطر الأرض بالكيلومتر
        const double R = 6371;

        // بنحول الدرجات لـ Radians
        // عشان المعادلة بتشتغل بـ Radians مش Degrees
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        // Haversine Formula نفسها
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // المسافة بالكيلومتر
        return Math.Round(R * c, 2);
    }

    private static double ToRadians(double degrees)
        => degrees * Math.PI / 180;
}