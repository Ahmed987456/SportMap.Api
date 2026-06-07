using Microsoft.AspNetCore.Http;

namespace SportMap.Application.Interfaces;

public interface IImageService
{
    Task<List<string>> UploadImagesAsync(int venueId, int ownerId, List<IFormFile> files);
    Task DeleteImageAsync(int imageId, int ownerId);
    Task SetPrimaryAsync(int imageId, int ownerId);
}