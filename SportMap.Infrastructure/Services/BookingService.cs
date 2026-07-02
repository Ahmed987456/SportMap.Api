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
    private readonly INotificationService _notificationService;

    public BookingService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    private static DateTime NowEgypt() =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time");

    public async Task<BookingResponse> CreateAsync(BookingRequest request, int playerId)
    {
        var now = NowEgypt();

        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == request.VenueId);

        if (venue == null) throw new Exception("Venue not found");
        if (!venue.IsApproved) throw new Exception("Venue is not approved yet");

        if (request.BookingDate < DateOnly.FromDateTime(now))
            throw new Exception("Cannot book a past date.");

        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == request.TimeSlotId && s.VenueId == request.VenueId);

        if (slot == null) throw new Exception("Time slot not found");

        // التحقق من انتهاء الميعاد
        var slotEndDateTime = request.BookingDate.ToDateTime(slot.EndTime);
        if (slot.EndTime <= slot.StartTime)
        {
            slotEndDateTime = slotEndDateTime.AddDays(1);
        }

        if (slotEndDateTime <= now)
            throw new Exception("This slot has already expired");

        if (!slot.IsAvailable)
            throw new Exception("Time slot is not available");

        // ✅ التحقق مفيش حجز نشط على نفس الـ Slot والتاريخ
        var alreadyBooked = await _context.Bookings.AnyAsync(b =>
            b.TimeSlotId == request.TimeSlotId &&
            b.BookingDate == request.BookingDate &&
            b.Status != BookingStatus.Cancelled &&
            !b.IsDeleted);

        if (alreadyBooked)
            throw new Exception("This slot is already booked for the selected date");

        // ✅ في حالة One-to-One، لازم نتأكد إن الـ Slot مالوش Booking تاني (حتى لو cancelled)
        // نستخدم AsNoTracking عشان نتجنب مشاكل التتبع
        var existingBookingOnSlot = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.TimeSlotId == request.TimeSlotId && !b.IsDeleted);

        if (existingBookingOnSlot != null && existingBookingOnSlot.Status != BookingStatus.Cancelled)
            throw new Exception("This slot is already booked");

        // حساب السعر
        var hours = (slot.EndTime - slot.StartTime).TotalHours;
        if (hours < 0) hours += 24;

        var totalPrice = Math.Round((decimal)hours * venue.PricePerHour, 2);
        var depositAmount = Math.Round(totalPrice * venue.DepositPercentage / 100, 2);

        var player = await _context.Users.FirstOrDefaultAsync(u => u.Id == playerId);

        var booking = new Booking
        {
            VenueId = request.VenueId,
            TimeSlotId = request.TimeSlotId,
            PlayerId = playerId,
            BookingDate = request.BookingDate,
            TotalPrice = totalPrice,
            DepositAmount = depositAmount,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.Bookings.Add(booking);

        // ✅ نخلي الـ Slot مش متاح
        slot.IsAvailable = false;
        slot.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            venue.OwnerId,
            "حجز جديد! 🎉",
            $"{player?.Name ?? "لاعب"} حجز {venue.Name}",
            $"/owner/venues/{venue.Id}/bookings"
        );

        return await GetBookingResponseAsync(booking.Id);
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(int playerId)
    {
        var now = NowEgypt();

        var bookings = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .Where(b => b.PlayerId == playerId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(b => ToResponse(b, now)).ToList();
    }

    public async Task<List<BookingResponse>> GetVenueBookingsAsync(int venueId, int ownerId)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);
        if (venue == null) throw new Exception("Venue not found");
        if (venue.OwnerId != ownerId) throw new Exception("Unauthorized");

        var bookings = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .Where(b => b.VenueId == venueId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(b => ToResponse(b, NowEgypt())).ToList();
    }

    public async Task CancelAsync(int bookingId, int playerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) throw new Exception("Booking not found");
        if (booking.PlayerId != playerId) throw new Exception("Unauthorized");
        if (booking.Status == BookingStatus.Cancelled) throw new Exception("Booking is already cancelled");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        booking.IsDeleted = true;  // ✅ Soft delete

        // ✅ نرجع الـ Slot يتاح تاني
        var slot = await _context.TimeSlots.FirstOrDefaultAsync(s => s.Id == booking.TimeSlotId);
        if (slot != null)
        {
            slot.IsAvailable = true;
            slot.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ConfirmAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) throw new Exception("Booking not found");
        if (booking.Venue.OwnerId != ownerId) throw new Exception("Unauthorized");
        if (booking.Status != BookingStatus.Pending) throw new Exception("Booking is not in pending state");

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            booking.PlayerId,
            "تم تأكيد حجزك ✅",
            $"حجزك في {booking.Venue.Name} يوم {booking.BookingDate} اتأكد!",
            "/player/bookings"
        );
    }

    public async Task SubmitPaymentAsync(int bookingId, int playerId, string paymentReference)
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new Exception("Payment reference is required");

        var normalized = NormalizeArabicNumbers(paymentReference.Trim());

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^\d{4}$"))
            throw new Exception("يجب إدخال آخر 4 أرقام فقط من رقم العملية");

        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) throw new Exception("Booking not found");
        if (booking.PlayerId != playerId) throw new Exception("Unauthorized");
        if (booking.PaymentStatus == PaymentStatus.Paid) throw new Exception("This booking is already paid");

        var referenceUsed = await _context.Bookings.AnyAsync(b =>
            b.PaymentReference == normalized && b.Id != bookingId);

        if (referenceUsed)
            throw new Exception("هذا الرقم المرجعي تم استخدامه من قبل");

        booking.PaymentReference = normalized;
        booking.PaymentStatus = PaymentStatus.PendingVerification;
        booking.PaymentSubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            booking.Venue.OwnerId,
            "في انتظار تأكيد دفع 💰",
            $"حجز جديد محتاج تأكيد دفع — آخر 4 أرقام: {normalized}",
            $"/owner/venues/{booking.VenueId}/bookings"
        );
    }

    public async Task ConfirmPaymentAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) throw new Exception("Booking not found");
        if (booking.Venue.OwnerId != ownerId) throw new Exception("Unauthorized");
        if (booking.PaymentStatus != PaymentStatus.PendingVerification) throw new Exception("No pending payment to confirm");

        booking.PaymentStatus = PaymentStatus.Paid;
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            booking.PlayerId,
            "تم تأكيد دفعك وحجزك ✅",
            $"تم تأكيد حجزك في {booking.Venue.Name} يوم {booking.BookingDate}",
            "/player/bookings"
        );
    }

    public async Task RejectPaymentAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) throw new Exception("Booking not found");
        if (booking.Venue.OwnerId != ownerId) throw new Exception("Unauthorized");
        if (booking.PaymentStatus != PaymentStatus.PendingVerification) throw new Exception("No pending payment to reject");

        booking.Status = BookingStatus.Cancelled;
        booking.PaymentStatus = PaymentStatus.Unpaid;
        booking.PaymentReference = null;
        booking.PaymentSubmittedAt = null;
        booking.UpdatedAt = DateTime.UtcNow;
        booking.IsDeleted = true;  // ✅ Soft delete

        // ✅ نرجع الـ Slot يتاح تاني
        var slot = await _context.TimeSlots.FirstOrDefaultAsync(s => s.Id == booking.TimeSlotId);
        if (slot != null)
        {
            slot.IsAvailable = true;
            slot.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            booking.PlayerId,
            "الرقم المرجعي غير صحيح ❌",
            $"تم إلغاء حجزك في {booking.Venue.Name} لأن الرقم المرجعي غير صحيح. احجز من جديد وأرسل الرقم الصحيح",
            "/player/bookings"
        );
    }

    private async Task<BookingResponse> GetBookingResponseAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return ToResponse(booking!, NowEgypt());
    }

    private static BookingResponse ToResponse(Booking booking, DateTime nowEgypt)
    {
        var slotEnd = booking.BookingDate.ToDateTime(booking.TimeSlot.EndTime);
        if (booking.TimeSlot.EndTime <= booking.TimeSlot.StartTime)
        {
            slotEnd = slotEnd.AddDays(1);
        }

        var isExpired = slotEnd <= nowEgypt && booking.Status != BookingStatus.Cancelled;

        return new BookingResponse
        {
            Id = booking.Id,
            VenueName = booking.Venue.Name,
            PlayerName = booking.Player.Name,
            StartTime = booking.TimeSlot.StartTime.ToString("HH:mm"),
            EndTime = booking.TimeSlot.EndTime.ToString("HH:mm"),
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            DepositAmount = Math.Round(booking.DepositAmount, 2),
            PaymentReference = booking.PaymentReference,
            Status = isExpired ? "Expired" : booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),
            VenueId = booking.VenueId,
            IsExpired = isExpired
        };
    }

    private static string NormalizeArabicNumbers(string input)
    {
        return input
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
            .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
            .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8')
            .Replace('٩', '9');
    }
}