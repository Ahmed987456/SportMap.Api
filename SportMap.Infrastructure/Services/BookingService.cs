using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Bookings;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;
using System.Data;

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

    public async Task<BookingResponse> CreateAsync(BookingRequest request, int playerId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow, "Egypt Standard Time");

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == request.VenueId);

            if (venue == null) throw new Exception("Venue not found");
            if (!venue.IsApproved) throw new Exception("Venue is not approved yet");

            if (request.BookingDate < DateOnly.FromDateTime(now))
                throw new Exception("Cannot book a past date.");

            // 🔒 Lock slot (منع race condition)
            var slot = await _context.TimeSlots
                .FirstOrDefaultAsync(s =>
                    s.Id == request.TimeSlotId &&
                    s.VenueId == request.VenueId);

            if (slot == null)
                throw new Exception("Time slot not found");

            if (!slot.IsAvailable)
                throw new Exception("Time slot is not available");

            // ⏱ check time
            if (request.BookingDate == DateOnly.FromDateTime(now))
            {
                var currentTime = TimeOnly.FromDateTime(now);
                if (slot.StartTime <= currentTime)
                    throw new Exception("This slot has already started or expired");
            }

            // 🚨 double booking check
            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.TimeSlotId == request.TimeSlotId &&
                b.BookingDate == request.BookingDate &&
                b.Status != BookingStatus.Cancelled);

            if (alreadyBooked)
                throw new Exception("This slot is already booked");

            var hours = (slot.EndTime - slot.StartTime).TotalHours;
            var totalPrice = (decimal)hours * venue.PricePerHour;
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

            // 🔥 أهم سطر في المشروع كله
            slot.IsAvailable = false;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            await _notificationService.SendToUserAsync(
                venue.OwnerId,
                "حجز جديد! 🎉",
                $"{player?.Name ?? "لاعب"} حجز {venue.Name}",
                $"/owner/venues/{venue.Id}/bookings"
            );

            return await GetBookingResponseAsync(booking.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.PlayerId != playerId)
            throw new Exception("Unauthorized");

        if (booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already cancelled");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        // 🔥 أهم سطر (ده اللي كان ناقص عندك)
        booking.TimeSlot.IsAvailable = true;

        await _context.SaveChangesAsync();
    }
    public async Task ConfirmAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Booking is not in pending state");

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // إشعار للاعب
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

        // ✅ تحويل الأرقام العربية للإنجليزية
        var normalized = NormalizeArabicNumbers(paymentReference.Trim());

        // ✅ تأكد إنها 4 أرقام بس
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^\d{4}$"))
            throw new Exception("يجب إدخال آخر 4 أرقام فقط من رقم العملية");

        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.PlayerId != playerId)
            throw new Exception("Unauthorized");

        if (booking.PaymentStatus == PaymentStatus.Paid)
            throw new Exception("This booking is already paid");

        // ✅ تأكد إن الرقم مش مستخدم قبل كده
        var referenceUsed = await _context.Bookings.AnyAsync(b =>
            b.PaymentReference == normalized &&
            b.Id != bookingId);

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

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        if (booking.PaymentStatus != PaymentStatus.PendingVerification)
            throw new Exception("No pending payment to confirm");

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

    private async Task<BookingResponse> GetBookingResponseAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return ToResponse(booking!);
    }

    public async Task RejectPaymentAsync(int bookingId, int ownerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        if (booking.PaymentStatus != PaymentStatus.PendingVerification)
            throw new Exception("No pending payment to reject");

        booking.Status = BookingStatus.Cancelled;
        booking.PaymentStatus = PaymentStatus.Unpaid;
        booking.PaymentReference = null;
        booking.PaymentSubmittedAt = null;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            booking.PlayerId,
            "الرقم المرجعي غير صحيح ❌",
            $"تم إلغاء حجزك في {booking.Venue.Name} لأن الرقم المرجعي غير صحيح. احجز من جديد وأرسل الرقم الصحيح",
            "/player/bookings"
        );
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
        DepositAmount = booking.DepositAmount,
        PaymentReference = booking.PaymentReference,
        Status = booking.Status.ToString(),
        PaymentStatus = booking.PaymentStatus.ToString(),
        VenueId = booking.VenueId,
    };

    // ✅ تحويل الأرقام العربية للإنجليزية
    private static string NormalizeArabicNumbers(string input)
    {
        return input
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
            .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
            .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8')
            .Replace('٩', '9');
    }
}