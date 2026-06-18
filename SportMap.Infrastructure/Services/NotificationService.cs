using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        // Initialize Firebase مرة واحدة بس
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

    public async Task SendToUserAsync(int userId, string title, string body)
    {
        // نجيب كل الـ Tokens بتاعت المستخدم
        var tokens = await _context.UserDevices
            .Where(d => d.UserId == userId)
            .Select(d => d.FcmToken)
            .ToListAsync();

        if (!tokens.Any()) return;

        foreach (var token in tokens)
        {
            try
            {
                var message = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            Sound = "default",
                            Priority = NotificationPriority.HIGH
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps { Sound = "default" }
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch
            {
                // لو الـ Token انتهى نحذفه
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

    public async Task RegisterDeviceAsync(int userId, string fcmToken)
    {
        // نتأكد مش مسجل قبل كده
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