using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly AppDbContext _context;
    private readonly Cloudinary _cloudinary;

    public ImageService(AppDbContext context, IConfiguration config)
    {
        _context = context;

        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<List<string>> UploadImagesAsync(
        int venueId, int ownerId, List<IFormFile> files)
    {
        var venue = await _context.Venues
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == venueId);

        if (venue == null)
            throw new Exception("Venue not found");

        if (venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        if (files.Count == 0)
            throw new Exception("No files uploaded");

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Only JPEG, PNG, and WebP images are allowed");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size must not exceed 5MB");

            // رفع على Cloudinary
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"sportmap/venues/{venueId}",
                Transformation = new Transformation()
                    .Width(800).Height(600).Crop("fill")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            // الـ URL بتاع Cloudinary
            var imageUrl = uploadResult.SecureUrl.ToString();

            var isPrimary = venue.Images.Count == 0 && uploadedUrls.Count == 0;

            var image = new VenueImage
            {
                VenueId = venueId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary
            };

            _context.VenueImages.Add(image);
            uploadedUrls.Add(imageUrl);
        }

        await _context.SaveChangesAsync();
        return uploadedUrls;
    }

    public async Task DeleteImageAsync(int imageId, int ownerId)
    {
        var image = await _context.VenueImages
            .Include(i => i.Venue)
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image == null)
            throw new Exception("Image not found");

        if (image.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        // حذف من Cloudinary
        var publicId = GetPublicIdFromUrl(image.ImageUrl);
        if (!string.IsNullOrEmpty(publicId))
            await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        image.IsDeleted = true;
        image.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SetPrimaryAsync(int imageId, int ownerId)
    {
        var image = await _context.VenueImages
            .Include(i => i.Venue)
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image == null)
            throw new Exception("Image not found");

        if (image.Venue.OwnerId != ownerId)
            throw new Exception("Unauthorized");

        var allImages = await _context.VenueImages
            .Where(i => i.VenueId == image.VenueId)
            .ToListAsync();

        foreach (var img in allImages)
            img.IsPrimary = false;

        image.IsPrimary = true;
        await _context.SaveChangesAsync();
    }

    private string GetPublicIdFromUrl(string url)
    {
        // بنجيب الـ Public ID من الـ URL
        // مثال: https://res.cloudinary.com/xxx/image/upload/v123/sportmap/venues/1/abc.jpg
        // Public ID: sportmap/venues/1/abc
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var uploadIndex = path.IndexOf("/upload/");
            if (uploadIndex < 0) return "";
            var afterUpload = path[(uploadIndex + 8)..];
            // شيل الـ version لو موجود
            if (afterUpload.StartsWith("v") && afterUpload.Contains('/'))
                afterUpload = afterUpload[(afterUpload.IndexOf('/') + 1)..];
            // شيل الـ extension
            var dotIndex = afterUpload.LastIndexOf('.');
            if (dotIndex > 0)
                afterUpload = afterUpload[..dotIndex];
            return afterUpload;
        }
        catch { return ""; }
    }
}