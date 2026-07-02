using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    // Foreign Key
    public int VenueId { get; set; }

    // Navigation Properties
    public Venue Venue { get; set; } = null!;
    public Booking? Booking { get; set; }
}