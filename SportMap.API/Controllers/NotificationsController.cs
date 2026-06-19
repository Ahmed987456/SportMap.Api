using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Notifications;
using SportMap.Application.Interfaces;
using System.Security.Claims;

namespace SportMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// 🔓 أي مستخدم — يجيب إشعاراته
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
    }

    /// <summary>
    /// 🔓 أي مستخدم — يعلّم إشعار كمقروء
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAsReadAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Marked as read"));
    }

    /// <summary>
    /// 🔓 أي مستخدم — يعلّم كل الإشعارات كمقروءة
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse<object>.Ok(null!, "All marked as read"));
    }
}