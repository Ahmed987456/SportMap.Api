using SportMap.Domain.Enums;

namespace SportMap.Application.DTOs.Venues;

public class VenueRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PricePerHour { get; set; }
    public VenueSurface Surface { get; set; }
    public int Capacity { get; set; }

    public int DepositPercentage { get; set; } = 50;

}