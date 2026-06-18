namespace SportMap.Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(int userId, string title, string body);
    Task RegisterDeviceAsync(int userId, string fcmToken);
}