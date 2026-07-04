using Microsoft.EntityFrameworkCore;
using SportMap.Application.DTOs.Venues;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;

    public FavoriteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(int playerId, int venueId)
    {
        var exists = await _context.Favorites
            .AnyAsync(f => f.PlayerId == playerId && f.VenueId == venueId);

        if (exists)
            throw new Exception("Venue is already in favorites");

        _context.Favorites.Add(new Favorite
        {
            PlayerId = playerId,
            VenueId = venueId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int playerId, int venueId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.PlayerId == playerId && f.VenueId == venueId);

        if (favorite == null)
            throw new Exception("Favorite not found");

        favorite.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<List<VenueResponse>> GetMyFavoritesAsync(int playerId)
    {
        var venues = await _context.Favorites
            .Where(f => f.PlayerId == playerId)
            .Include(f => f.Venue)
                .ThenInclude(v => v.Owner)
            .Include(f => f.Venue)
                .ThenInclude(v => v.Images)
            .Select(f => f.Venue)
            .ToListAsync();

        return venues.Select(ToVenueResponse).ToList();
    }

    public async Task<bool> IsFavoriteAsync(int playerId, int venueId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.PlayerId == playerId && f.VenueId == venueId);
    }

    private static VenueResponse ToVenueResponse(Venue venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        Description = venue.Description,
        Address = venue.Address,
        Latitude = venue.Latitude,
        Longitude = venue.Longitude,
        PricePerHour = venue.PricePerHour,
        Surface = venue.Surface.ToString(),
        Capacity = venue.Capacity,
        IsApproved = venue.IsApproved,
        IsDeleted = venue.IsDeleted,
        OwnerName = venue.Owner.Name,
        DepositPercentage = venue.DepositPercentage,
        OwnerVodafoneCash = venue.Owner.VodafoneCashNumber,
        OwnerInstaPay = venue.Owner.InstaPayNumber,
        PrimaryImageUrl = venue.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
        Images = venue.Images
            .Where(i => !i.IsDeleted)
            .Select(i => new VenueImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary
            })
            .ToList()
    };
}