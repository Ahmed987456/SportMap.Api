using SportMap.Application.DTOs.Reviews;

namespace SportMap.Application.Interfaces;

public interface IReviewService
{
    // اللاعب يضيف Review
    Task<ReviewResponse> CreateAsync(ReviewRequest request, int playerId);

    // يشوف Reviews ملعب معين
    Task<List<ReviewResponse>> GetVenueReviewsAsync(int venueId);
}