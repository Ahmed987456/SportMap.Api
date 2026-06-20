namespace SportMap.Application.DTOs.Venues;

public class OwnerDashboardResponse
{
    public int TotalVenues { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
}