using SportMap.Domain.Common;
using SportMap.Domain.Enums;
using System;

namespace SportMap.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Player;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation Properties
    public ICollection<Venue> Venues { get; set; } = new List<Venue>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}