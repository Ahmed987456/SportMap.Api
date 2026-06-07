using SportMap.Domain.Enums;

namespace SportMap.Application.DTOs.Venues;

public class VenueFilter
{
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Geo Filter
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public double RadiusInKm { get; set; } = 10;

    // Search
    public string? Search { get; set; }

    // Filters
    public VenueSurface? Surface { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}