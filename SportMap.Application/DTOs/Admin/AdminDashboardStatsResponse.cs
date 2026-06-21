namespace SportMap.Application.DTOs.Admin;

public class AdminDashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalVenues { get; set; }
    public int TotalBookings { get; set; }

    public int PendingVenues { get; set; }
    public int ApprovedVenues { get; set; }
    public int SuspendedVenues { get; set; }
}