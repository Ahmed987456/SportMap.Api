using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    // Foreign Keys
    public int BookingId { get; set; }
    public int VenueId { get; set; }
    public int PlayerId { get; set; }

    // Navigation Properties
    public Booking Booking { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public User Player { get; set; } = null!;
}