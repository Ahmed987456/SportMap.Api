using SportMap.Application.DTOs.Notifications;

namespace SportMap.Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(int userId, string title, string body, string? link = null);
    Task RegisterDeviceAsync(int userId, string fcmToken);
    Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}