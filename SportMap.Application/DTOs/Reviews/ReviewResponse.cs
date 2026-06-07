namespace SportMap.Application.DTOs.Reviews;

public class ReviewResponse
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}