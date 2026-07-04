using SportMap.Domain.Common;

namespace SportMap.Domain.Entities;

public class Favorite : BaseEntity
{
    public int PlayerId { get; set; }
    public int VenueId { get; set; }

    public User Player { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
}