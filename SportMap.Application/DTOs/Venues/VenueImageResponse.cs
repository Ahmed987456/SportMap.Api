namespace SportMap.Application.DTOs.Venues;

public class VenueImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}