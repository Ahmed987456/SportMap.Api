using SportMap.Application.DTOs.Bookings;

namespace SportMap.Application.Interfaces;

public interface IBookingService
{
    // اللاعب يحجز
    Task<BookingResponse> CreateAsync(BookingRequest request, int playerId);

    // اللاعب يشوف حجوزاته
    Task<List<BookingResponse>> GetMyBookingsAsync(int playerId);

    // صاحب الملعب يشوف حجوزات ملعبه
    Task<List<BookingResponse>> GetVenueBookingsAsync(int venueId, int ownerId);

    // اللاعب يلغي حجزه
    Task CancelAsync(int bookingId, int playerId);

    // صاحب الملعب يوافق على الحجز
    Task ConfirmAsync(int bookingId, int ownerId);

    Task SubmitPaymentAsync(int bookingId, int playerId, string paymentReference);
    Task ConfirmPaymentAsync(int bookingId, int ownerId);

    Task RejectPaymentAsync(int bookingId, int ownerId);

    Task<VenueRevenueSummaryResponse> GetOwnerRevenueAsync(int ownerId);
}