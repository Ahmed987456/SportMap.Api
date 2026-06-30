using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Bookings;
using SportMap.Application.DTOs.Common;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

/// <summary>
/// 🎫 إدارة الحجوزات
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// ⚽ لاعب فقط — يحجز ملعب في ميعاد معين
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Create(BookingRequest request)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _bookingService.CreateAsync(request, playerId);
        return Ok(ApiResponse<BookingResponse>.Ok(booking, "Booking created successfully"));
    }

    /// <summary>
    /// ⚽ لاعب فقط — يشوف كل حجوزاته السابقة والجاية
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> GetMyBookings()
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bookings = await _bookingService.GetMyBookingsAsync(playerId);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(bookings));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يشوف كل حجوزات ملعبه بس
    /// </summary>
    [HttpGet("venue/{venueId}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> GetVenueBookings(int venueId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bookings = await _bookingService.GetVenueBookingsAsync(venueId, ownerId);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(bookings));
    }

    /// <summary>
    /// ⚽ لاعب فقط — يلغي حجزه (مش ممكن يلغي حجز حد تاني)
    /// </summary>
    [HttpPatch("{bookingId}/cancel")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Cancel(int bookingId)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _bookingService.CancelAsync(bookingId, playerId);
        return Ok(ApiResponse<object>.Ok(null!, "Booking cancelled successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يوافق على حجز في ملعبه بس
    /// </summary>
    [HttpPatch("{bookingId}/confirm")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Confirm(int bookingId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _bookingService.ConfirmAsync(bookingId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Booking confirmed successfully"));
    }

    /// <summary>
    /// ⚽ لاعب فقط — يبعت الرقم المرجعي بعد ما يحوّل العربون
    /// </summary>
    [HttpPost("{bookingId}/submit-payment")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> SubmitPayment(int bookingId, [FromBody] SubmitPaymentRequest request)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _bookingService.SubmitPaymentAsync(bookingId, playerId, request.PaymentReference);
        return Ok(ApiResponse<object>.Ok(null!, "Payment reference submitted, awaiting confirmation"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يأكد إنه استلم التحويل
    /// </summary>
    [HttpPatch("{bookingId}/confirm-payment")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> ConfirmPayment(int bookingId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _bookingService.ConfirmPaymentAsync(bookingId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Payment confirmed"));
    }
}