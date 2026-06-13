using SportMap.Domain.Enums;

namespace SportMap.Application.DTOs.Venues;

public class VenueResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PricePerHour { get; set; }
    public string Surface { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsApproved { get; set; }

    public bool IsDeleted { get; set; }
    public string OwnerName { get; set; } = string.Empty;

    // ✅ جديد — المسافة بالكيلومتر من موقع اللاعب
    // Nullable لأن مش دايماً اللاعب بيبعت موقعه
    public double? DistanceInKm { get; set; }

    public string? PrimaryImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();

}