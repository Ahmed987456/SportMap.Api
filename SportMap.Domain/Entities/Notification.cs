using SportMap.Domain.Common;
using SportMap.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? Link { get; set; } // ← جديد
    public User User { get; set; } = null!;
}