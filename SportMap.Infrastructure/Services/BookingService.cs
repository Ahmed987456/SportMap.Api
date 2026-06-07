using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Bookings;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponse> CreateAsync(BookingRequest request, int playerId)
    {
        // 1. نتأكد إن الملعب موجود ومتوافق عليه
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (!venue.IsApproved)
            throw new Exception("Venue is not approved yet");

        // 2. نتأكد إن الـ Slot موجود وتابع للملعب ده ومتاح
        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(s =>
                s.Id == request.TimeSlotId &&
                s.VenueId == request.VenueId);

        if (slot == null)
            throw new Exception("Time slot not found");

        if (!slot.IsAvailable)
            throw new Exception("Time slot is not available");

        // 3. نتأكد إن نفس الـ Slot مش محجوز في نفس التاريخ
        // يعني ملعب + slot + تاريخ = وحيد
        var alreadyBooked = await _context.Bookings.AnyAsync(b =>
            b.TimeSlotId == request.TimeSlotId &&
            b.BookingDate == request.BookingDate &&
            b.Status != BookingStatus.Cancelled);

        if (alreadyBooked)
            throw new Exception("This slot is already booked for the selected date");

        // 4. نحسب السعر تلقائي
        // الفرق بين StartTime و EndTime بالساعات × السعر في الساعة
        var hours = (slot.EndTime - slot.StartTime).TotalHours;
        var totalPrice = (decimal)hours * venue.PricePerHour;

        // 5. نعمل الحجز
        var booking = new Booking
        {
            VenueId = request.VenueId,
            TimeSlotId = request.TimeSlotId,
            PlayerId = playerId,
            BookingDate = request.BookingDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return await GetBookingResponseAsync(booking.Id);
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(int playerId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .Where(b => b.PlayerId == playerId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(ToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetVenueBookingsAsync(int venueId, int ownerId)
    {
        // نتأكد إن الملعب ده فعلاً بتاعه
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var bookings = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .Where(b => b.VenueId == venueId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(ToResponse).ToList();
    }

    public async Task CancelAsync(int bookingId, int playerId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        // اللاعب يقدر يلغي حجزه بس مش حجز غيره
        if (booking.PlayerId != playerId)
            throw new Exception("Unauthorized");

        if (booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already cancelled");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ConfirmAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        // صاحب الملعب يقدر يوافق على حجوزات ملعبه بس
        if (booking.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Booking is not in pending state");

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ===== Private Helpers =====

    private async Task<BookingResponse> GetBookingResponseAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return ToResponse(booking!);
    }

    private static BookingResponse ToResponse(Booking booking) => new()
    {
        Id = booking.Id,
        VenueName = booking.Venue.Name,
        PlayerName = booking.Player.Name,
        StartTime = booking.TimeSlot.StartTime.ToString("HH:mm"),
        EndTime = booking.TimeSlot.EndTime.ToString("HH:mm"),
        BookingDate = booking.BookingDate,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status.ToString(),
        PaymentStatus = booking.PaymentStatus.ToString()
    };
}