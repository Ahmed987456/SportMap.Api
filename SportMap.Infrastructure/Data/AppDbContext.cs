using Microsoft.EntityFrameworkCore;
using SportMap.Domain.Entities;

namespace SportMap.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<VenueImage> VenueImages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft Delete Filters
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Venue>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<TimeSlot>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Booking>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<VenueImage>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<UserDevice>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(x => !x.IsDeleted);

        // Booking Relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Player)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Venue)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.TimeSlot)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review Relationships
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Player)
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Venue)
            .WithMany(v => v.Reviews)
            .HasForeignKey(r => r.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal Precision
        modelBuilder.Entity<Venue>()
            .Property(x => x.PricePerHour)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(x => x.TotalPrice)
            .HasPrecision(10, 2);
    }
}