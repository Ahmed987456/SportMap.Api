using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Admin;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Interfaces;

namespace SportMap.API.Controllers;

/// <summary>
/// 👑 كل الـ Endpoints دي للأدمن بس
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف الملاعب اللي لسه مستنية موافقة
    /// </summary>
    [HttpGet("venues/pending")]
    public async Task<IActionResult> GetPendingVenues()
    {
        var venues = await _adminService.GetPendingVenuesAsync();
        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل الملاعب حتى المحذوفة والموقوفة
    /// </summary>
    [HttpGet("venues")]
    public async Task<IActionResult> GetAllVenues()
    {
        var venues = await _adminService.GetAllVenuesAsync();
        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }

    /// <summary>
    /// 👑 Admin Dashboard Statistics
    /// </summary>
    [HttpGet("dashboard-stats")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();

        return Ok(
            ApiResponse<AdminDashboardStatsResponse>.Ok(stats)
        );
    }
    /// <summary>
    /// 👑 Approved Venues
    /// </summary>
    [HttpGet("approved")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetApprovedVenues()
    {
        var venues = await _adminService.GetApprovedVenuesAsync();

        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }
    /// <summary>
    /// 👑 Suspended Venues
    /// </summary>
    [HttpGet("suspended")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetSuspendedVenues()
    {
        var venues = await _adminService.GetSuspendedVenuesAsync();

        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }
    /// <summary>
    /// 👑 أدمن فقط — يوافق على ملعب عشان يظهر للاعبين
    /// </summary>
    [HttpPatch("venues/{venueId}/approve")]
    public async Task<IActionResult> ApproveVenue(int venueId)
    {
        await _adminService.ApproveVenueAsync(venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Venue approved successfully"));
    }

    /// <summary>
    /// 👑 أدمن فقط — يوقف ملعب ويخفيه من اللاعبين
    /// </summary>
    [HttpPatch("venues/{venueId}/suspend")]
    public async Task<IActionResult> SuspendVenue(int venueId)
    {
        await _adminService.SuspendVenueAsync(venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Venue suspended successfully"));
    }
}