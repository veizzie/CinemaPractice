using Microsoft.AspNetCore.Http;
namespace CinemaWeb.Services
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);
        bool DeleteImage(string fileName);
        string GetImagePath(string fileName);
    }
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
        public string FilePath { get; set; }
    }
}