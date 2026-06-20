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
    private readonly INotificationService _notificationService;

    public ReviewService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ReviewResponse> CreateAsync(ReviewRequest request, int playerId)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new Exception("Rating must be between 1 and 5");

        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Player)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.PlayerId != playerId)
            throw new Exception("Unauthorized");

        if (booking.Status != BookingStatus.Confirmed)
            throw new Exception("Can only review confirmed bookings");

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.BookingId == request.BookingId);

        if (alreadyReviewed)
            throw new Exception("You already reviewed this booking");

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

        // إشعار لصاحب الملعب
        await _notificationService.SendToUserAsync(
            booking.Venue.OwnerId,
            "تقييم جديد ⭐",
            $"{booking.Player.Name} عمل تقييم {request.Rating}/5 على {booking.Venue.Name}",
            $"/owner/venues/{booking.VenueId}"
        );

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