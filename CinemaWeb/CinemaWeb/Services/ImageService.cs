using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;

        private const string ImagesFolder = "images";
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif"
        };

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return ErrorResult("Файл не вибрано");
                }

                if (file.Length > MaxFileSize)
                {
                    var sizeInMb = MaxFileSize / (1024 * 1024);
                    return ErrorResult($"Файл занадто великий (макс. {sizeInMb}MB)");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) ||
                    !AllowedExtensions.Contains(extension))
                {
                    var allowed = string.Join(", ", AllowedExtensions);
                    return ErrorResult($"Дозволені формати: {allowed}");
                }

                var uniqueFileName = GenerateUniqueFileName(extension);
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    ImagesFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var stream = new FileStream(
                    filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return new ImageUploadResult
                {
                    Success = true,
                    FileName = uniqueFileName,
                    FilePath = $"/{ImagesFolder}/{uniqueFileName}"
                };
            }
            catch (Exception ex)
            {
                return ErrorResult($"Помилка: {ex.Message}");
            }
        }

        public bool DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            try
            {
                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    ImagesFolder,
                    fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetImagePath(string fileName)
        {
            return $"/{ImagesFolder}/{fileName}";
        }

        private string GenerateUniqueFileName(string extension)
        {
            return $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
        }

        private ImageUploadResult ErrorResult(string message)
        {
            return new ImageUploadResult
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}