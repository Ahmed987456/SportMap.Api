namespace SportMap.Application.DTOs.Slots;

public class SlotRequest
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}