using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class VenueImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;

    // Foreign Key
    public int VenueId { get; set; }

    // Navigation Property
    public Venue Venue { get; set; } = null!;
}