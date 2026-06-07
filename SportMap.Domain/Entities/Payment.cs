using SportMap.Domain.Common;
using SportMap.Domain.Enums;

namespace SportMap.Domain.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
    public string? TransactionId { get; set; }

    // Foreign Key
    public int BookingId { get; set; }

    // Navigation Property
    public Booking Booking { get; set; } = null!;
}