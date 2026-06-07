using SportMap.Domain.Common;
using SportMap.Domain.Enums;

namespace SportMap.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PricePerHour { get; set; }
    public VenueSurface Surface { get; set; }
    public int Capacity { get; set; }
    public bool IsApproved { get; set; } = false;

    // Foreign Key
    public int OwnerId { get; set; }

    // Navigation Properties
    public User Owner { get; set; } = null!;
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<VenueImage> Images { get; set; } = new List<VenueImage>();
}