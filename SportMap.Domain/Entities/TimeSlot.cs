using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Foreign Key
    public int VenueId { get; set; }

    // Navigation Properties
    public Venue Venue { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}