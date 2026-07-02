namespace SportMap.Application.DTOs.Slots;

public class SlotResponse
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int VenueId { get; set; }
}