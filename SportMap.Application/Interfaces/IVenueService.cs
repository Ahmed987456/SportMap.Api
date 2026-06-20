using SportMap.Application.DTOs.Common;
using SportMap.Application.DTOs.Venues;

namespace SportMap.Application.Interfaces;

public interface IVenueService
{
    // استبدلنا GetAllAsync القديمة بالجديدة
    Task<PagedResponse<VenueResponse>> GetAllAsync(VenueFilter filter);

    Task<List<VenueResponse>> GetMyVenuesAsync(int ownerId);
    Task<VenueResponse> GetByIdAsync(int id);
    Task<VenueResponse> CreateAsync(VenueRequest request, int ownerId);
    Task<VenueResponse> UpdateAsync(int id, VenueRequest request, int ownerId);
    Task DeleteAsync(int id, int ownerId);

    Task<OwnerDashboardResponse> GetOwnerDashboardAsync(int ownerId);

    Task<OwnerAnalyticsResponse> GetAnalyticsAsync(int ownerId);

}
