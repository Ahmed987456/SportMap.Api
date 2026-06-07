namespace SportMap.Application.DTOs.Bookings;

public class BookingRequest
{
    public int VenueId { get; set; }
    public int TimeSlotId { get; set; }
    // بنبعت التاريخ بس، الوقت موجود في الـ TimeSlot
    public DateOnly BookingDate { get; set; }
}