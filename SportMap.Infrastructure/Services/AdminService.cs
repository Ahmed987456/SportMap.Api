using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Admin;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Interfaces;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public AdminService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<VenueResponse>> GetPendingVenuesAsync()
    {
        // بنجيب الملاعب اللي IsApproved = false
        // وبنجيب معاها الـ Owner عشان نعرض اسمه
        return await _context.Venues
            .Include(v => v.Owner)
            .Where(v => !v.IsApproved)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }

    public async Task<List<VenueResponse>> GetAllVenuesAsync()
    {
        // الأدمن يشوف كل الملاعب حتى اللي مش Approved
        // عشان كده بنستخدم IgnoreQueryFilters
        // اللي بيتجاهل الـ Filter اللي عملناه في DbContext
        return await _context.Venues
            .IgnoreQueryFilters()
            .Include(v => v.Owner)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }

    public async Task ApproveVenueAsync(int venueId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.IsApproved)
            throw new Exception("Venue is already approved");

        venue.IsApproved = true;
        venue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // إشعار لصاحب الملعب
        await _notificationService.SendToUserAsync(
            venue.OwnerId,
            "تم اعتماد ملعبك ✅",
            $"تم الموافقة على ملعب {venue.Name} وأصبح متاحاً للحجز!",
            $"/owner/venues"
        );
    }

    public async Task SuspendVenueAsync(int venueId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        // Suspend = نرجع IsApproved لـ false
        // الملعب هيختفي من البحث تلقائي
        venue.IsApproved = false;
        venue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<AdminDashboardStatsResponse> GetDashboardStatsAsync()
    {
        var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);

        var weeklyBookings = await _context.Bookings
            .Where(x => x.CreatedAt >= startOfWeek)
            .GroupBy(x => x.CreatedAt.DayOfWeek)
            .Select(g => new Application.DTOs.Admin.WeeklyBookingDto
            {
                Day = g.Key.ToString(),
                Bookings = g.Count()
            })
            .ToListAsync();

        return new AdminDashboardStatsResponse
        {
            TotalUsers = await _context.Users.CountAsync(),

            TotalPlayers = await _context.Users
                .CountAsync(x => x.Role == UserRole.Player),

            TotalOwners = await _context.Users
                .CountAsync(x => x.Role == UserRole.VenueOwner),

            TotalVenues = await _context.Venues.CountAsync(),

            TotalBookings = await _context.Bookings.CountAsync(),

            PendingVenues = await _context.Venues
                .CountAsync(x => !x.IsApproved && !x.IsDeleted),

            ApprovedVenues = await _context.Venues
                .CountAsync(x => x.IsApproved && !x.IsDeleted),

            SuspendedVenues = await _context.Venues
                .IgnoreQueryFilters()
                .CountAsync(x => !x.IsApproved && x.IsDeleted),

            WeeklyBookings = weeklyBookings,

            UsersDistribution = new UsersDistributionDto
            {
                Players = await _context.Users.CountAsync(x => x.Role == UserRole.Player),
                Owners = await _context.Users.CountAsync(x => x.Role == UserRole.VenueOwner),
                Admins = await _context.Users.CountAsync(x => x.Role == UserRole.SuperAdmin)
            }
        };
    }
    public async Task<List<VenueResponse>> GetApprovedVenuesAsync()
    {
        return await _context.Venues
            .Include(v => v.Owner)
            .Include(v => v.Images)
            .Where(v => v.IsApproved)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }
    public async Task<List<VenueResponse>> GetSuspendedVenuesAsync()
    {
        return await _context.Venues
            .IgnoreQueryFilters()
            .Include(v => v.Owner)
            .Include(v => v.Images)
            .Where(v => !v.IsApproved && v.IsDeleted)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }

    // نفس الـ ToResponse اللي في VenueService
    // بس هنا محتاجينها عشان AdminService مش بيورث منه

    public async Task ResetDemoDataAsync()
    {
        await _context.Notifications.ExecuteDeleteAsync();

        await _context.Reviews.ExecuteDeleteAsync();

        await _context.Bookings.ExecuteDeleteAsync();

        await _context.TimeSlots.ExecuteDeleteAsync();

        await _context.VenueImages.ExecuteDeleteAsync();

        await _context.Venues.ExecuteDeleteAsync();

        await _context.UserDevices.ExecuteDeleteAsync();

        await _context.Users
            .Where(x => x.Role != UserRole.SuperAdmin)
            .ExecuteDeleteAsync();
    }
    private static VenueResponse ToResponse(Domain.Entities.Venue venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        Description = venue.Description,
        Address = venue.Address,
        Latitude = venue.Latitude,
        Longitude = venue.Longitude,
        PricePerHour = venue.PricePerHour,
        Surface = venue.Surface.ToString(),
        Capacity = venue.Capacity,

        IsApproved = venue.IsApproved,
        IsDeleted = venue.IsDeleted,

        OwnerName = venue.Owner.Name
    };
}