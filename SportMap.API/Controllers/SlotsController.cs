using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Slots;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

[ApiController]
[Route("api/venues/{venueId}/slots")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;

    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    /// <summary>
    /// 🔓 متاح للكل — يشوف المواعيد المتاحة في تاريخ معين
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSlots(int venueId, [FromQuery] DateOnly date)
    {
        var slots = await _slotService.GetVenueSlotsAsync(venueId, date);
        return Ok(ApiResponse<List<SlotResponse>>.Ok(slots));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يشوف كل المواعيد
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> GetAllSlots(int venueId, [FromQuery] DateOnly? date)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var slots = await _slotService.GetAllVenueSlotsAsync(venueId, ownerId, date);
        return Ok(ApiResponse<List<SlotResponse>>.Ok(slots));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يضيف ميعاد جديد
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Create(int venueId, SlotRequest request)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var slot = await _slotService.CreateAsync(venueId, request, ownerId);
        return Ok(ApiResponse<SlotResponse>.Ok(slot, "Slot created successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يحذف ميعاد
    /// </summary>
    [HttpDelete("{slotId}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Delete(int venueId, int slotId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _slotService.DeleteAsync(slotId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Slot deleted successfully"));
    }

    /// <summary>
    /// 🏟️ صاحب ملعب فقط — يوقف أو يفتح ميعاد
    /// </summary>
    [HttpPatch("{slotId}/toggle")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> Toggle(int venueId, int slotId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _slotService.ToggleAvailabilityAsync(slotId, ownerId);
        return Ok(ApiResponse<object>.Ok(null!, "Slot availability updated"));
    }
}