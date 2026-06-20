using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportMap.Application.DTOs.Notifications;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context, IConfiguration config)
    {
        _context = context;

        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialJson = config["Firebase:CredentialJson"];
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential
                    .FromJson(credentialJson)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            });
        }
    }

    public async Task SendToUserAsync(int userId, string title, string body, string? link = null)
    {
        // حفظ في Database
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            IsRead = false,
            Link = link 
        });
        await _context.SaveChangesAsync();

        // بعت Push Notification
        var tokens = await _context.UserDevices
            .Where(d => d.UserId == userId)
            .Select(d => d.FcmToken)
            .ToListAsync();

        foreach (var token in tokens)
        {
            try
            {
                var message = new Message
                {
                    Token = token,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    }
                };
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch
            {
                var device = await _context.UserDevices
                    .FirstOrDefaultAsync(d => d.FcmToken == token);
                if (device != null)
                {
                    device.IsDeleted = true;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                Link = n.Link
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            throw new Exception("Notification not found");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
            n.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task RegisterDeviceAsync(int userId, string fcmToken)
    {
        var exists = await _context.UserDevices
            .AnyAsync(d => d.UserId == userId && d.FcmToken == fcmToken);

        if (exists) return;

        _context.UserDevices.Add(new UserDevice
        {
            UserId = userId,
            FcmToken = fcmToken
        });

        await _context.SaveChangesAsync();
    }
}