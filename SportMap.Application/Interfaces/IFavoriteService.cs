using SportMap.Application.DTOs.Venues;

namespace SportMap.Application.Interfaces;

public interface IFavoriteService
{
    Task AddAsync(int playerId, int venueId);
    Task RemoveAsync(int playerId, int venueId);
    Task<List<VenueResponse>> GetMyFavoritesAsync(int playerId);
    Task<bool> IsFavoriteAsync(int playerId, int venueId);
}