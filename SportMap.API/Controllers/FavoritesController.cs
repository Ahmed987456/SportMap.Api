using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Player")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var favorites = await _favoriteService.GetMyFavoritesAsync(playerId);
        return Ok(ApiResponse<List<VenueResponse>>.Ok(favorites));
    }

    [HttpPost("{venueId}")]
    public async Task<IActionResult> Add(int venueId)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.AddAsync(playerId, venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Added to favorites"));
    }

    [HttpDelete("{venueId}")]
    public async Task<IActionResult> Remove(int venueId)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.RemoveAsync(playerId, venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Removed from favorites"));
    }

    [HttpGet("{venueId}/check")]
    public async Task<IActionResult> IsFavorite(int venueId)
    {
        var playerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isFav = await _favoriteService.IsFavoriteAsync(playerId, venueId);
        return Ok(ApiResponse<bool>.Ok(isFav));
    }
}