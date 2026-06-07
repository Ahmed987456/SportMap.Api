namespace SportMap.Application.DTOs.Slots;

public class SlotRequest
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}