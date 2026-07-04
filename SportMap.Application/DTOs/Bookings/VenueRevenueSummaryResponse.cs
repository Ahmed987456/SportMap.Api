using SportMap.Application.DTOs.Bookings;

public class VenueRevenueSummaryResponse
{
    public decimal ThisMonthRevenue { get; set; }
    public decimal LastMonthRevenue { get; set; }
    public decimal PercentageChange { get; set; }
    public List<MonthlyRevenueResponse> MonthlyBreakdown { get; set; } = new();
}