using SportMap.Application.DTOs.Venues;

namespace SportMap.Application.Interfaces;

public interface IAdminService
{
    // يشوف الملاعب اللي لسه متوافقش عليها
    Task<List<VenueResponse>> GetPendingVenuesAsync();

    // يشوف كل الملاعب
    Task<List<VenueResponse>> GetAllVenuesAsync();

    // يوافق على ملعب
    Task ApproveVenueAsync(int venueId);

    // يوقف ملعب
    Task SuspendVenueAsync(int venueId);
}