using SportMap.Application.DTOs.Slots;

namespace SportMap.Application.Interfaces;

public interface ISlotService
{
    Task<List<SlotResponse>> GetVenueSlotsAsync(int venueId, DateOnly? date);
    Task<SlotResponse> CreateAsync(int venueId, SlotRequest request, int ownerId);
    Task<List<SlotResponse>> GetAllVenueSlotsAsync(int venueId, int ownerId);
    Task DeleteAsync(int slotId, int ownerId);
    Task ToggleAvailabilityAsync(int slotId, int ownerId);
}