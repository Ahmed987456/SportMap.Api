using SportMap.Domain.Common;
using SportMap.Domain.Enums;

namespace SportMap.Domain.Entities;

public class Booking : BaseEntity
{
    public DateOnly BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal DepositAmount { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? PaymentSubmittedAt { get; set; }

    // Foreign Keys
    public int PlayerId { get; set; }
    public int VenueId { get; set; }
    public int TimeSlotId { get; set; }

    // Navigation Properties
    public User Player { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public Review? Review { get; set; }
}