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
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var cutoff = DateTime.UtcNow.AddMinutes(-30);

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

                    await notificationService.SendToUserAsync(
                        booking.PlayerId,
                        "تم إلغاء حجزك تلقائياً ⏰",
                        $"تم إلغاء حجزك في {booking.Venue.Name} بسبب عدم الدفع",
                        "/player/bookings"
                    );
                }

                if (expiredBookings.Any())
                    await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BookingCleanupService Error: " + ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}