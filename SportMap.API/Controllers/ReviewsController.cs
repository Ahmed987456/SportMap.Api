using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Reviews;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

/// <summary>
/// ⭐ إدارة التقييمات
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// ⚽ لاعب فقط — يضيف تقييم على ملعب حجزه وتأكد (مرة واحدة بس لكل حجز)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Create(ReviewRequest request)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var review = await _reviewService.CreateAsync(request, playerId);
        return Ok(ApiResponse<ReviewResponse>.Ok(review, "Review added successfully"));
    }

    /// <summary>
    /// 🔓 متاح للكل — يشوف تقييمات ملعب معين بدون Login
    /// </summary>
    [HttpGet("venue/{venueId}")]
    public async Task<IActionResult> GetVenueReviews(int venueId)
    {
        var reviews = await _reviewService.GetVenueReviewsAsync(venueId);
        return Ok(ApiResponse<List<ReviewResponse>>.Ok(reviews));
    }
}