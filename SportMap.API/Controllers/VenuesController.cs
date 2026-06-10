using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

/// <summary>
/// 🏟️ إدارة الملاعب
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    /// <summary>
    /// 🔓 متاح للكل — يشوف كل الملاعب المتاحة مع فلاتر وترقيم صفحات
    /// يقدر يفلتر بـ: موقع / سعر / نوع أرض / بحث باسم أو عنوان
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] VenueFilter filter)
    {
        var result = await _venueService.GetAllAsync(filter);
        return Ok(ApiResponse<PagedResponse<VenueResponse>>.Ok(result));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يشوف ملاعبه هو بس
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> GetMyVenues()
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var venues = await _venueService.GetMyVenuesAsync(ownerId);
        return Ok(ApiResponse<List<VenueResponse>>.Ok(venues));
    }

    /// <summary>
    /// 🔓 متاح للكل — يشوف تفاصيل ملعب معين بالصور والمعلومات الكاملة
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var venue = await _venueService.GetByIdAsync(id);
        return Ok(ApiResponse<VenueResponse>.Ok(venue));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يضيف ملعب جديد (هيتحتاج موافقة الأدمن قبل ما يظهر)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Create(VenueRequest request)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var venue = await _venueService.CreateAsync(request, ownerId);
        return Ok(ApiResponse<VenueResponse>.Ok(venue, "Venue created successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يعدل بيانات ملعبه بس مش ملعب حد تاني
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Update(int id, VenueRequest request)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var venue = await _venueService.UpdateAsync(id, request, ownerId);
        return Ok(ApiResponse<VenueResponse>.Ok(venue, "Venue updated successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يحذف ملعبه بس مش ملعب حد تاني (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _venueService.DeleteAsync(id, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Venue deleted successfully"));
    }
}