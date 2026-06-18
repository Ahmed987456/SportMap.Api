using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class UserDevice : BaseEntity
{
    public int UserId { get; set; }
    public string FcmToken { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}