using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Reviews;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewResponse> CreateAsync(ReviewRequest request, int playerId)
    {
        // 1. نتأكد إن الـ Rating بين 1 و 5
        if (request.Rating < 1 || request.Rating > 5)
            throw new Exception("Rating must be between 1 and 5");

        // 2. نجيب الـ Booking ونتأكد إنه بتاع اللاعب ده
        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        // 3. نتأكد إن الـ Booking بتاع اللاعب ده بالظبط
        if (booking.PlayerId != playerId)
            throw new Exception("Unauthorized");

        // 4. نتأكد إن الحجز Confirmed
        // منطقي مش هيعمل Review على حجز ملغي أو لسه Pending
        if (booking.Status != BookingStatus.Confirmed)
            throw new Exception("Can only review confirmed bookings");

        // 5. نتأكد إنه مش عامل Review على نفس الـ Booking قبل كده
        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.BookingId == request.BookingId);

        if (alreadyReviewed)
            throw new Exception("You already reviewed this booking");

        // 6. نعمل الـ Review
        // الـ VenueId بنجيبه من الـ Booking مش من المستخدم
        var review = new Review
        {
            BookingId = request.BookingId,
            VenueId = booking.VenueId,
            PlayerId = playerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await GetReviewResponseAsync(review.Id);
    }

    public async Task<List<ReviewResponse>> GetVenueReviewsAsync(int venueId)
    {
        return await _context.Reviews
            .Include(r => r.Player)
            .Include(r => r.Venue)
            .Where(r => r.VenueId == venueId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => ToResponse(r))
            .ToListAsync();
    }

    // ===== Private Helpers =====

    private async Task<ReviewResponse> GetReviewResponseAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.Player)
            .Include(r => r.Venue)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        return ToResponse(review!);
    }

    private static ReviewResponse ToResponse(Review review) => new()
    {
        Id = review.Id,
        PlayerName = review.Player.Name,
        VenueName = review.Venue.Name,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}