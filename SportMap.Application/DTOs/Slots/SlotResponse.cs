using SportMap.Domain.Enums;

namespace SportMap.Application.DTOs.Slots;

public class SlotResponse
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public int VenueId { get; set; }
    public string Status { get; set; } = "متاح";  // ✅ جديد: متاح / محجوز / منتهي
}