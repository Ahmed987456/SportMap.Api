using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SportMap.Application.Interfaces;
using SportMap.Domain.Entities;
using SportMap.Infrastructure.Data;

namespace SportMap.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ImageService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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

        // الحل: بنستخدم ContentRootPath بدل WebRootPath
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "venues", venueId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Only JPEG, PNG, and WebP images are allowed");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size must not exceed 5MB");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var imageUrl = $"/uploads/venues/{venueId}/{fileName}";

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

        // نمسح الملف من الديسك
        var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
        if (File.Exists(filePath))
            File.Delete(filePath);

        // Soft Delete في الـ Database
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

        // نشيل الـ Primary من كل الصور
        var allImages = await _context.VenueImages
            .Where(i => i.VenueId == image.VenueId)
            .ToListAsync();

        foreach (var img in allImages)
            img.IsPrimary = false;

        // نحط الـ Primary على الصورة دي بس
        image.IsPrimary = true;
        await _context.SaveChangesAsync();
    }
}