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
        private readonly string _imagesFolder = "images";
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Зміна типу повернення для відповідності Інтерфейсу
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Файл не вибрано"
                    };
                }

                if (file.Length > _maxFileSize)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = $"Файл занадто великий (макс. {_maxFileSize / (1024 * 1024)}MB)"
                    };
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = $"Дозволені формати: {string.Join(", ", _allowedExtensions)}"
                    };
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, _imagesFolder);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Повертаємо успішний об'єкт
                return new ImageUploadResult
                {
                    Success = true,
                    FileName = uniqueFileName,
                    FilePath = $"/{_imagesFolder}/{uniqueFileName}" // Зручно мати повний шлях одразу
                };
            }
            catch (Exception ex)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Помилка: {ex.Message}"
                };
            }
        }

        public bool DeleteImage(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, _imagesFolder, fileName);

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
            return $"/{_imagesFolder}/{fileName}";
        }
    }
}