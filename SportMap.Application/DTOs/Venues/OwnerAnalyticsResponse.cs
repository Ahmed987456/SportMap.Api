namespace SportMap.Application.DTOs.Venues;

public class OwnerAnalyticsResponse
{
    public List<WeeklyBookingDto> WeeklyBookings { get; set; } = new();

    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

    public List<TopVenueDto> TopVenues { get; set; } = new();
}

public class WeeklyBookingDto
{
    public string Day { get; set; } = "";
    public int Bookings { get; set; }
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = "";
    public decimal Revenue { get; set; }
}

public class TopVenueDto
{
    public string VenueName { get; set; } = "";
    public int Bookings { get; set; }
}