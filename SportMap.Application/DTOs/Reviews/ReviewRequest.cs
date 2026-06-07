namespace SportMap.Application.DTOs.Reviews;

public class ReviewRequest
{
    public int BookingId { get; set; }
    // الـ VenueId هنجيبه من الـ Booking مش من المستخدم
    // عشان نتأكد إن الـ Booking ده فعلاً بتاعه

    public int Rating { get; set; }
    // من 1 لـ 5 وهنعمل Validation عليه

    public string Comment { get; set; } = string.Empty;
}