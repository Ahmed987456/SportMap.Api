namespace SportMap.Application.DTOs.Admin;

public class VenueListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal PricePerHour { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}