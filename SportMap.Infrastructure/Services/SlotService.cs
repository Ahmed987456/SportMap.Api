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

    private static DateTime NowEgypt() =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time");

    public async Task<List<SlotResponse>> GetVenueSlotsAsync(int venueId, DateOnly date)
    {
        var now = NowEgypt();
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        var slots = await _context.TimeSlots
            .Where(s =>
                s.VenueId == venueId &&
                s.Date == date &&
                s.IsAvailable &&
                // ✅ نشوف المواعيد اللي لسه مجاتش بس
                (date > today || (date == today && s.EndTime > currentTime)))
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // ✅ نشيل المواعيد المحجوزة
        var bookedSlotIds = await _context.Bookings
            .Where(b =>
                b.VenueId == venueId &&
                b.BookingDate == date &&
                b.Status != BookingStatus.Cancelled)
            .Select(b => b.TimeSlotId)
            .ToListAsync();

        return slots
            .Where(s => !bookedSlotIds.Contains(s.Id))
            .Select(ToResponse)
            .ToList();
    }

    public async Task<List<SlotResponse>> GetAllVenueSlotsAsync(int venueId, int ownerId)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null) throw new Exception("Venue not found");
        if (venue.OwnerId != ownerId) throw new Exception("Unauthorized");

        var today = DateOnly.FromDateTime(NowEgypt());

        // ✅ صاحب الملعب يشوف كل المواعيد من النهارده وبعدها
        var slots = await _context.TimeSlots
    .Where(s => s.VenueId == venueId && s.Date >= today)
    .OrderBy(s => s.Date)
    .ThenBy(s => s.StartTime)
    .ToListAsync();

        return slots.Select(ToResponse).ToList();
    }

    public async Task<SlotResponse> CreateAsync(int venueId, SlotRequest request, int ownerId)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null) throw new Exception("Venue not found");
        if (venue.OwnerId != ownerId) throw new Exception("Unauthorized");

        var now = NowEgypt();
        var today = DateOnly.FromDateTime(now);

        // ✅ التاريخ مش في الماضي
        if (request.Date < today)
            throw new Exception("لا يمكن إضافة مواعيد في الماضي");

        // ✅ الوقت صح
        if (request.StartTime >= request.EndTime)
        {
            // ✅ حل مشكلة 11 مساء → 12 صباح (اليوم التالي)
            // لو EndTime أصغر من StartTime معناه الميعاد بيعدي منتصف الليل
            // مش بنسمح بيه عشان يسبب تعقيد
            throw new Exception("وقت البداية يجب أن يكون قبل وقت النهاية في نفس اليوم");
        }

        // ✅ الميعاد مش في الماضي (بالتوقيت المصري)
        if (request.Date == today)
        {
            var currentTime = TimeOnly.FromDateTime(now);
            if (request.EndTime <= currentTime)
                throw new Exception("هذا الميعاد انتهى بالفعل");
        }

        // ✅ مفيش Overlap
        var overlap = await _context.TimeSlots.AnyAsync(s =>
            s.VenueId == venueId &&
            s.Date == request.Date &&
            !s.IsDeleted &&
            s.StartTime < request.EndTime &&
            s.EndTime > request.StartTime);

        if (overlap)
            throw new Exception("هذا الميعاد يتعارض مع ميعاد موجود");

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

        if (slot == null) throw new Exception("Slot not found");
        if (slot.Venue.OwnerId != ownerId) throw new Exception("Unauthorized");

        slot.IsDeleted = true;
        slot.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleAvailabilityAsync(int slotId, int ownerId)
    {
        var slot = await _context.TimeSlots
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == slotId);

        if (slot == null) throw new Exception("Slot not found");
        if (slot.Venue.OwnerId != ownerId) throw new Exception("Unauthorized");

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