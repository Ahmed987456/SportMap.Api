namespace SportMap.Application.DTOs.Bookings;

public class MonthlyRevenueResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int BookingsCount { get; set; }
}