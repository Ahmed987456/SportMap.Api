using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

/// <summary>
/// 🖼️ إدارة صور الملاعب — صاحب ملعب فقط
/// </summary>
[ApiController]
[Route("api/venues/{venueId}/images")]
[Authorize(Roles = "VenueOwner")]
public class VenueImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public VenueImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يرفع صور لملعبه بس (JPEG, PNG, WebP — ماكس 5MB للصورة)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upload(int venueId, [FromForm] List<IFormFile> files)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var urls = await _imageService.UploadImagesAsync(venueId, ownerId, files);
        return Ok(ApiResponse<List<string>>.Ok(urls, "Images uploaded successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يحذف صورة من ملعبه بس
    /// </summary>
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> Delete(int venueId, int imageId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _imageService.DeleteImageAsync(imageId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Image deleted successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يحدد صورة كـ Primary تظهر أول في الملعب
    /// </summary>
    [HttpPatch("{imageId}/primary")]
    public async Task<IActionResult> SetPrimary(int venueId, int imageId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _imageService.SetPrimaryAsync(imageId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Primary image updated"));
    }
}