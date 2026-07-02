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

    public async Task<List<SlotResponse>> GetVenueSlotsAsync(int venueId, DateOnly date)
    {
        var egyptNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.UtcNow, "Egypt Standard Time");

        var currentTime = TimeOnly.FromDateTime(egyptNow);
        var today = DateOnly.FromDateTime(egyptNow);

        var slots = await _context.TimeSlots
            .Where(s =>
                s.VenueId == venueId &&
                s.Date == date &&
                s.IsAvailable &&
                (date > today || (date == today && s.StartTime > currentTime)))
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        var bookedSlotIds = await _context.Bookings
            .Where(b =>
                b.VenueId == venueId &&
                b.BookingDate == date &&
                b.Status != BookingStatus.Cancelled)
            .Select(b => b.TimeSlotId)
            .ToListAsync();

        return slots
            .Where(s => !bookedSlotIds.Contains(s.Id))
            .Select(s => ToResponse(s))
            .ToList();
    }

    public async Task<List<SlotResponse>> GetAllVenueSlotsAsync(int venueId, int ownerId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
     DateTime.UtcNow, "Egypt Standard Time"));

        return await _context.TimeSlots
            .Where(s => s.VenueId == venueId && s.Date >= today)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .Select(s => ToResponse(s))
            .ToListAsync();
    }

    public async Task<SlotResponse> CreateAsync(int venueId, SlotRequest request, int ownerId)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var egyptNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
    DateTime.UtcNow, "Egypt Standard Time");
        var today = DateOnly.FromDateTime(egyptNow);

        if (request.Date < today)
            throw new Exception("Cannot add slots in the past");

        if (request.StartTime >= request.EndTime)
            throw new Exception("Start time must be before end time");

        var overlap = await _context.TimeSlots.AnyAsync(s =>
            s.VenueId == venueId &&
            s.Date == request.Date &&
            s.StartTime < request.EndTime &&
            s.EndTime > request.StartTime);

        if (overlap)
            throw new Exception("This time slot overlaps with an existing slot");

        var slot = new TimeSlot
        {
            VenueId = venueId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = true
        };

        _context.TimeSlots.Add(slot);
        await _context.SaveChangesAsync();

        return ToResponse(slot);
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
        Date = slot.Date,
        StartTime = slot.StartTime.ToString("HH:mm"),
        EndTime = slot.EndTime.ToString("HH:mm"),
        IsAvailable = slot.IsAvailable,
        VenueId = slot.VenueId
    };
}