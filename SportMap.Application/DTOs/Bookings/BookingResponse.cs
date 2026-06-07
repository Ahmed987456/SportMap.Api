namespace SportMap.Application.DTOs.Bookings;

public class BookingResponse
{
    public int Id { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}