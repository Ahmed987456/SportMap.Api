using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Helpers;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;
using System.Globalization;
namespace SportMap.Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public VenueService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<PagedResponse<VenueResponse>> GetAllAsync(VenueFilter filter) 
    {
        var query = _context.Venues
            .Include(v => v.Owner)
            .Include(v => v.Images) // ← ضيفنا Images
            .Where(v => v.IsApproved)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(v =>
                v.Name.Contains(filter.Search) ||
                v.Address.Contains(filter.Search));

        if (filter.Surface.HasValue)
            query = query.Where(v => v.Surface == filter.Surface);

        if (filter.MinPrice.HasValue)
            query = query.Where(v => v.PricePerHour >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(v => v.PricePerHour <= filter.MaxPrice);

        var venues = await query.ToListAsync();

        if (filter.Lat.HasValue && filter.Lng.HasValue)
        {
            venues = venues
                .Select(v => new
                {
                    Venue = v,
                    Distance = GeoHelper.CalculateDistanceInKm(
                        filter.Lat.Value, filter.Lng.Value,
                        v.Latitude, v.Longitude)
                })
                .Where(x => x.Distance <= filter.RadiusInKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Venue)
                .ToList();
        }

        var totalCount = venues.Count;

        var pagedVenues = venues
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var responses = pagedVenues.Select(v =>
        {
            var response = ToResponse(v);
            if (filter.Lat.HasValue && filter.Lng.HasValue)
            {
                response.DistanceInKm = GeoHelper.CalculateDistanceInKm(
                    filter.Lat.Value, filter.Lng.Value,
                    v.Latitude, v.Longitude);
            }
            return response;
        }).ToList();

        return new PagedResponse<VenueResponse>
        {
            Data = responses,
            CurrentPage = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }
    public async Task<List<VenueResponse>> GetMyVenuesAsync(int ownerId)
    {
        return await _context.Venues
            .Include(v => v.Owner)
            .Include(v => v.Images)
            .Where(v => v.OwnerId == ownerId)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }
    public async Task<VenueResponse> GetByIdAsync(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.Owner)
            .Include(v => v.Images) // ← ضيفنا Images
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue == null)
            throw new Exception("Venue not found");

        return ToResponse(venue);
    }

    public async Task<VenueResponse> CreateAsync(VenueRequest request, int ownerId)
    {
        var venue = new Venue
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PricePerHour = request.PricePerHour,
            Surface = request.Surface,
            Capacity = request.Capacity,
            OwnerId = ownerId,
            IsApproved = false
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        // إشعار للأدمن
        var owner = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        var admins = await _context.Users
            .Where(u => u.Role == UserRole.SuperAdmin)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var adminId in admins)
        {
            await _notificationService.SendToUserAsync(
                adminId,
                "ملعب جديد ينتظر موافقتك 🏟️",
                $"{owner?.Name ?? "صاحب ملعب"} أضاف ملعب جديد: {venue.Name}",
                "/admin"
            );
        }

        return await GetByIdAsync(venue.Id);
    }

    public async Task<VenueResponse> UpdateAsync(int id, VenueRequest request, int ownerId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        venue.Name = request.Name;
        venue.Description = request.Description;
        venue.Address = request.Address;
        venue.Latitude = request.Latitude;
        venue.Longitude = request.Longitude;
        venue.PricePerHour = request.PricePerHour;
        venue.Surface = request.Surface;
        venue.Capacity = request.Capacity;
        venue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(venue.Id);
    }

    public async Task DeleteAsync(int id, int ownerId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        venue.IsDeleted = true;
        venue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<OwnerDashboardResponse> GetOwnerDashboardAsync(int ownerId)
    {
        var venueIds = await _context.Venues
            .Where(v => v.OwnerId == ownerId)
            .Select(v => v.Id)
            .ToListAsync();

        var totalVenues = venueIds.Count;

        var bookings = await _context.Bookings
            .Where(b => venueIds.Contains(b.VenueId))
            .ToListAsync();

        var totalBookings = bookings.Count;

        var totalRevenue = bookings
            .Where(b => b.PaymentStatus == Domain.Enums.PaymentStatus.Paid)
            .Sum(b => b.TotalPrice);

        var reviews = await _context.Reviews
            .Where(r => venueIds.Contains(r.VenueId))
            .ToListAsync();

        var averageRating = reviews.Any()
            ? reviews.Average(r => r.Rating)
            : 0;

        return new OwnerDashboardResponse
        {
            TotalVenues = totalVenues,
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            AverageRating = averageRating
        };
    }
    public async Task<OwnerAnalyticsResponse> GetAnalyticsAsync(int ownerId)
    {
        var venueIds = await _context.Venues
            .Where(v => v.OwnerId == ownerId)
            .Select(v => v.Id)
            .ToListAsync();

        var bookings = await _context.Bookings
            .Include(b => b.Venue)
            .Where(b => venueIds.Contains(b.VenueId))
            .ToListAsync();

        var daysOrder = new[]
{
    DayOfWeek.Saturday,
    DayOfWeek.Sunday,
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday,
    DayOfWeek.Friday
};

        var dayNames = new Dictionary<DayOfWeek, string>
{
    { DayOfWeek.Saturday, "Sat" },
    { DayOfWeek.Sunday, "Sun" },
    { DayOfWeek.Monday, "Mon" },
    { DayOfWeek.Tuesday, "Tue" },
    { DayOfWeek.Wednesday, "Wed" },
    { DayOfWeek.Thursday, "Thu" },
    { DayOfWeek.Friday, "Fri" }
};

        var grouped = bookings
            .GroupBy(b => b.BookingDate.ToDateTime(TimeOnly.MinValue).DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        var weeklyBookings = daysOrder
            .Select(day => new WeeklyBookingDto
            {
                Day = dayNames[day],
                Bookings = grouped.ContainsKey(day) ? grouped[day] : 0
            })
            .ToList();
        var monthlyRevenue = bookings
            .Where(b => b.PaymentStatus == PaymentStatus.Paid)
            .GroupBy(b => b.BookingDate.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                Revenue = g.Sum(x => x.TotalPrice)
            })
            .ToList();

        var topVenues = bookings
            .GroupBy(b => b.Venue.Name)
            .Select(g => new TopVenueDto
            {
                VenueName = g.Key,
                Bookings = g.Count()
            })
            .OrderByDescending(x => x.Bookings)
            .Take(5)
            .ToList();

        return new OwnerAnalyticsResponse
        {
            WeeklyBookings = weeklyBookings,
            MonthlyRevenue = monthlyRevenue,
            TopVenues = topVenues
        };
    }
    private static VenueResponse ToResponse(Venue venue) => new()
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
        OwnerName = venue.Owner.Name,
        PrimaryImageUrl = venue.Images
        .FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
        Images = venue.Images
        .Where(i => !i.IsDeleted)
        .Select(i => new VenueImageResponse
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            IsPrimary = i.IsPrimary
        })
        .ToList()
    };
}