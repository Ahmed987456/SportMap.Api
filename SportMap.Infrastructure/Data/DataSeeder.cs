using SportMap.Domain.Entities;
using SportMap.Domain.Enums;

namespace SportMap.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // لو في Users موجودين خليص متضيفش تاني
        if (context.Users.Any()) return;

        // ===== Users =====
        var admin = new User
        {
            Name = "Ahmed Oraby Admin",
            Email = "ahmedoraby57000@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
            Phone = "01000000000",
            Role = UserRole.SuperAdmin
        };

        var owner1 = new User
        {
            Name = "Ahmed Owner",
            Email = "ahmed@owner.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
            Phone = "01011111111",
            Role = UserRole.VenueOwner
        };

        var owner2 = new User
        {
            Name = "Mohamed Owner",
            Email = "mohamed@owner.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
            Phone = "01022222222",
            Role = UserRole.VenueOwner
        };

        var player1 = new User
        {
            Name = "Omar Player",
            Email = "omar@player.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
            Phone = "01033333333",
            Role = UserRole.Player
        };

        var player2 = new User
        {
            Name = "Ali Player",
            Email = "ali@player.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
            Phone = "01044444444",
            Role = UserRole.Player
        };

        context.Users.AddRange(admin, owner1, owner2, player1, player2);
        await context.SaveChangesAsync();

        // ===== Venues =====
        // كلها في القاهرة في مناطق مختلفة
        var venue1 = new Venue
        {
            Name = "ملعب النصر",
            Description = "ملعب كرة قدم 7 أشخاص بإضاءة ليلية",
            Address = "المعادي، القاهرة",
            Latitude = 29.9602,
            Longitude = 31.2569,
            PricePerHour = 200,
            Surface = VenueSurface.Turf,
            Capacity = 14,
            IsApproved = true,
            OwnerId = owner1.Id
        };

        var venue2 = new Venue
        {
            Name = "ملعب الأهلي",
            Description = "ملعب كرة قدم 5 أشخاص أرضية طبيعية",
            Address = "مدينة نصر، القاهرة",
            Latitude = 30.0626,
            Longitude = 31.3219,
            PricePerHour = 150,
            Surface = VenueSurface.Grass,
            Capacity = 10,
            IsApproved = true,
            OwnerId = owner1.Id
        };

        var venue3 = new Venue
        {
            Name = "ملعب الزمالك",
            Description = "ملعب مغلق بتكييف هواء",
            Address = "الزمالك، القاهرة",
            Latitude = 30.0626,
            Longitude = 31.2197,
            PricePerHour = 300,
            Surface = VenueSurface.Indoor,
            Capacity = 12,
            IsApproved = true,
            OwnerId = owner2.Id
        };

        var venue4 = new Venue
        {
            Name = "ملعب الهرم",
            Description = "ملعب كرة قدم 7 أشخاص قريب من الأهرامات",
            Address = "الهرم، الجيزة",
            Latitude = 29.9773,
            Longitude = 31.1325,
            PricePerHour = 175,
            Surface = VenueSurface.Turf,
            Capacity = 14,
            IsApproved = true,
            OwnerId = owner2.Id
        };

        var venue5 = new Venue
        {
            Name = "ملعب التجمع",
            Description = "ملعب حديث في التجمع الخامس",
            Address = "التجمع الخامس، القاهرة الجديدة",
            Latitude = 30.0071,
            Longitude = 31.4913,
            PricePerHour = 250,
            Surface = VenueSurface.Turf,
            Capacity = 14,
            IsApproved = true,
            OwnerId = owner1.Id
        };

        context.Venues.AddRange(venue1, venue2, venue3, venue4, venue5);
        await context.SaveChangesAsync();

        // ===== TimeSlots =====
        // كل ملعب عنده Slots في أيام مختلفة
        var slots = new List<TimeSlot>();

        foreach (var venue in new[] { venue1, venue2, venue3, venue4, venue5 })
        {
            // من الأحد للخميس
            foreach (var day in new[] {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday })
            {
                slots.AddRange(new[]
                {
                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(8, 0),
                        EndTime = new TimeOnly(10, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(10, 0),
                        EndTime = new TimeOnly(12, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(16, 0),
                        EndTime = new TimeOnly(18, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(18, 0),
                        EndTime = new TimeOnly(20, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(20, 0),
                        EndTime = new TimeOnly(22, 0), IsAvailable = true },
                });
            }

            // الجمعة والسبت
            foreach (var day in new[] { DayOfWeek.Friday, DayOfWeek.Saturday })
            {
                slots.AddRange(new[]
                {
                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(10, 0),
                        EndTime = new TimeOnly(12, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(16, 0),
                        EndTime = new TimeOnly(18, 0), IsAvailable = true },

                    new TimeSlot { VenueId = venue.Id, DayOfWeek = day,
                        StartTime = new TimeOnly(20, 0),
                        EndTime = new TimeOnly(22, 0), IsAvailable = true },
                });
            }
        }

        context.TimeSlots.AddRange(slots);
        await context.SaveChangesAsync();
    }
}