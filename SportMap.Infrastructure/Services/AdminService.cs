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
        .Where(v => !v.IsApproved && !v.IsDeleted)
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
       .Where(v => v.IsApproved && !v.IsDeleted)
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

        // ✅ إشعار لصاحب الملعب
        await _notificationService.SendToUserAsync(
            venue.OwnerId,
            "تم إيقاف ملعبك ⚠️",
            $"تم إيقاف ملعب {venue.Name} من قبل الإدارة. تواصل معنا لمزيد من المعلومات",
            "/owner/venues"
        );
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

    public async Task<List<UserListResponse>> GetAllPlayersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.Player)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<UserListResponse>> GetAllOwnersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.VenueOwner)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<VenueListResponse>> GetAllVenuesListAsync()
    {
        return await _context.Venues
            .IgnoreQueryFilters()
            .Include(v => v.Owner)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VenueListResponse
            {
                Id = v.Id,
                Name = v.Name,
                OwnerName = v.Owner.Name,
                OwnerPhone = v.Owner.Phone,
                Address = v.Address,
                PricePerHour = v.PricePerHour,
                IsApproved = v.IsApproved,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }
    // نفس الـ ToResponse اللي في VenueService
    // بس هنا محتاجينها عشان AdminService مش بيورث منه

    public async Task ResetDemoDataAsync()
    {
        // الترتيب مهم جداً: نمسح الجداول اللي "بتشاور" على جداول تانية الأول
        await _context.Notifications.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _context.Reviews.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _context.Bookings.IgnoreQueryFilters().ExecuteDeleteAsync();      // ✅ لازم تتمسح قبل TimeSlots و Venues
        await _context.TimeSlots.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _context.VenueImages.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _context.Venues.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _context.UserDevices.IgnoreQueryFilters().ExecuteDeleteAsync();

        await _context.Users
            .IgnoreQueryFilters()
            .Where(x => x.Role != UserRole.SuperAdmin)
            .ExecuteDeleteAsync();

        await ResetSequenceAsync("Notifications");
        await ResetSequenceAsync("Reviews");
        await ResetSequenceAsync("Bookings");
        await ResetSequenceAsync("TimeSlots");
        await ResetSequenceAsync("VenueImages");
        await ResetSequenceAsync("Venues");
        await ResetSequenceAsync("UserDevices");

        await ResetUsersSequenceAsync();
    }

    public async Task<List<VenueResponse>> GetDeletedVenuesAsync()
    {
        return await _context.Venues
            .IgnoreQueryFilters()
            .Include(v => v.Owner)
            .Where(v => v.IsDeleted)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }

    public async Task PromoteToAdminAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        if (user.Role == UserRole.SuperAdmin)
            throw new Exception("User is already an admin");

        user.Role = UserRole.SuperAdmin;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task ResetSequenceAsync(string tableName)
    {
        await _context.Database.ExecuteSqlRawAsync(
            $"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH 1;"
        );
    }

    private async Task ResetUsersSequenceAsync()
    {
        var maxId = await _context.Users.IgnoreQueryFilters().MaxAsync(u => (int?)u.Id) ?? 0;
        var nextId = maxId + 1;

        await _context.Database.ExecuteSqlRawAsync(
            $"ALTER SEQUENCE \"Users_Id_seq\" RESTART WITH {nextId};"
        );
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