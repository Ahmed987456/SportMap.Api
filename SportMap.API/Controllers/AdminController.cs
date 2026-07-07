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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPendingVenues()
    {
        var venues = await _adminService.GetPendingVenuesAsync();
        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل الملاعب حتى المحذوفة والموقوفة
    /// </summary>
    [HttpGet("venues")]
    [Authorize(Roles = "SuperAdmin")]
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveVenue(int venueId)
    {
        await _adminService.ApproveVenueAsync(venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Venue approved successfully"));
    }

    /// <summary>
    /// 👑 أدمن فقط — يوقف ملعب ويخفيه من اللاعبين
    /// </summary>
    [HttpPatch("venues/{venueId}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SuspendVenue(int venueId)
    {
        await _adminService.SuspendVenueAsync(venueId);
        return Ok(ApiResponse<object>.Ok(null!, "Venue suspended successfully"));
    }

    /// <summary>
    /// أدمن فقط — يعمل ريسيت للداتا بيز
    /// </summary>
    [HttpPost("reset-demo-data")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ResetDemoData()
    {
        await _adminService.ResetDemoDataAsync();

        return Ok(new
        {
            success = true,
            message = "Demo data has been cleared."
        });
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل اللاعبين
    /// </summary>
    [HttpGet("players")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllPlayers()
    {
        var players = await _adminService.GetAllPlayersAsync();
        return Ok(ApiResponse<List<UserListResponse>>.Ok(players));
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل أصحاب الملاعب
    /// </summary>
    [HttpGet("owners")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllOwners()
    {
        var owners = await _adminService.GetAllOwnersAsync();
        return Ok(ApiResponse<List<UserListResponse>>.Ok(owners));
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل الملاعب
    /// </summary>
    [HttpGet("venues-list")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllVenuesList()
    {
        var venues = await _adminService.GetAllVenuesListAsync();
        return Ok(ApiResponse<List<VenueListResponse>>.Ok(venues));
    }

    /// <summary>
    /// 👑 أدمن فقط — يشوف كل الملاعب المحذوفه
    /// </summary>
    [HttpGet("venues/deleted")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetDeletedVenues()
    {
        var venues = await _adminService.GetDeletedVenuesAsync();
        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }

    /// <summary>
    /// 👑 أدمن فقط — يرقي مستخدم (لاعب أو صاحب ملعب) ليصبح أدمن
    /// </summary>
    [HttpPatch("users/{userId}/promote")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PromoteToAdmin(int userId)
    {
        await _adminService.PromoteToAdminAsync(userId);
        return Ok(ApiResponse<object>.Ok(null!, "User promoted to admin successfully"));
    }
}