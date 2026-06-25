using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Slots;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class SlotService : ISlotService
{
    private readonly AppDbContext _context;

    public SlotService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SlotResponse>> GetVenueSlotsAsync(int venueId, DateOnly? date)
    {
        // لو مش بعت تاريخ نستخدم النهارده
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // نجيب اليوم من التاريخ
        var dayOfWeek = targetDate.DayOfWeek;

        // نجيب الـ Slots المتاحة في نفس اليوم
        var now = DateTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);

        var slots = await _context.TimeSlots
            .Where(s =>
    !s.IsDeleted &&
    s.VenueId == venueId &&
    s.IsAvailable &&
    s.DayOfWeek == dayOfWeek &&
    (
        targetDate > DateOnly.FromDateTime(now)
        ||
        (
            targetDate == DateOnly.FromDateTime(now)
            &&
            s.StartTime > currentTime
        )
    ))
            .ToListAsync();

        // نشيل اللي اتحجز في التاريخ ده
        var bookedSlotIds = await _context.Bookings
            .Where(b =>
                b.VenueId == venueId &&
                b.BookingDate == targetDate &&
                b.Status != BookingStatus.Cancelled)
            .Select(b => b.TimeSlotId)
            .ToListAsync();

        return slots
            .Where(s => !bookedSlotIds.Contains(s.Id))
            .Select(s => ToResponse(s))
            .ToList();
    }

    public async Task<SlotResponse> CreateAsync(int venueId, SlotRequest request, int ownerId)
    {
        throw new Exception("TEST TEST TEST");

        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var today = DateTime.Now;

        if (request.DayOfWeek == today.DayOfWeek)
        {
            if (request.StartTime <= TimeOnly.FromDateTime(today))
                throw new Exception("Cannot create slot in the past.");
        }

        var overlap = await _context.TimeSlots.AnyAsync(s =>
            !s.IsDeleted &&
            s.VenueId == venueId &&
            s.DayOfWeek == request.DayOfWeek &&
            s.StartTime < request.EndTime &&
            s.EndTime > request.StartTime);

        if (overlap)
            throw new Exception("Time slot overlaps with an existing slot");

        var slot = new TimeSlot
        {
            VenueId = venueId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DayOfWeek = request.DayOfWeek,
            IsAvailable = true
        };

        _context.TimeSlots.Add(slot);
        await _context.SaveChangesAsync();

        return ToResponse(slot);
    }

    public async Task<List<SlotResponse>> GetAllVenueSlotsAsync(int venueId, int ownerId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);
        var todayDay = DateTime.Now.DayOfWeek;

        return await _context.TimeSlots
            .Where(s =>
                s.VenueId == venueId &&
                (
                    // الأيام اللي بعد النهارده
                    s.DayOfWeek != todayDay ||

                    // النهارده لكن الميعاد لسه مجاش
                    s.StartTime > nowTime
                )
            )
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => ToResponse(s))
            .ToListAsync();
    }
    public async Task DeleteAsync(int slotId, int ownerId)
    {
        var slot = await _context.TimeSlots
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == slotId);

        if (slot == null)
            throw new Exception("Slot not found");

        if (slot.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        slot.IsDeleted = true;
        slot.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleAvailabilityAsync(int slotId, int ownerId)
    {
        var slot = await _context.TimeSlots
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == slotId);

        if (slot == null)
            throw new Exception("Slot not found");

        if (slot.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        slot.IsAvailable = !slot.IsAvailable;
        slot.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static SlotResponse ToResponse(TimeSlot slot) => new()
    {
        Id = slot.Id,
        StartTime = slot.StartTime.ToString("HH:mm"),
        EndTime = slot.EndTime.ToString("HH:mm"),
        DayOfWeek = slot.DayOfWeek.ToString(),
        IsAvailable = slot.IsAvailable,
        VenueId = slot.VenueId
    };
}