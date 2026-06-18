using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public DeviceController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// 🔓 أي مستخدم — يسجّل الـ FCM Token بتاعه عشان يستقبل إشعارات
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromBody] string fcmToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.RegisterDeviceAsync(userId, fcmToken);
        return Ok(ApiResponse<object>.Ok(null!, "Device registered successfully"));
    }
}