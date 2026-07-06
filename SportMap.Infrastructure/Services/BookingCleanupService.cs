using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SportMap.Application.Interfaces;
using SportMap.Domain.Enums;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class BookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BookingCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notificationService = scope.ServiceProvider
                    .GetRequiredService<INotificationService>();

                var egyptNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow, "Egypt Standard Time");

                var cutoff = DateTime.UtcNow.AddMinutes(-5);

                var expiredBookings = await context.Bookings
                    .Include(b => b.Venue)
                    .Where(b =>
                        b.Status == BookingStatus.Pending &&
                        b.PaymentStatus == PaymentStatus.Unpaid &&
                        b.CreatedAt <= cutoff)
                    .ToListAsync(stoppingToken);

                foreach (var booking in expiredBookings)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.UpdatedAt = DateTime.UtcNow;

                    try
                    {
                        await notificationService.SendToUserAsync(
                            booking.PlayerId,
                            "تم إلغاء حجزك تلقائياً ⏰",
                            $"تم إلغاء حجزك في {booking.Venue.Name} لأنك لم تكمل الدفع في الوقت المحدد",
                            "/player/bookings?tab=cancelled"
                        );
                    }
                    catch { }
                }

                if (expiredBookings.Count > 0)
                    await context.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}