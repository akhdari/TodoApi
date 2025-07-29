using TodoApi.Models;
using TodoApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TodoApi.Services
{
    public class FileUploadService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadService(AppDbContext db, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<string>> SaveUploadedFiles(List<IFormFile> files, int userId)
        {
            // Use wwwroot as base, fallback if needed
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var publicUrls = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var fileName = Path.GetFileName(file.FileName);
                var userFolder = Path.Combine(rootPath, "uploads", userId.ToString());
                var storagePath = Path.Combine(userFolder, fileName);

                Directory.CreateDirectory(userFolder);

                if (File.Exists(storagePath))
                {
                    Console.WriteLine($"File {fileName} already exists for user {userId}. Overwriting.");
                    continue;
                }

                using (var stream = new FileStream(storagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store file record in DB
                var relativePath = $"uploads/{userId}/{fileName}";
                var uploadedFile = new UploadedFile
                {
                    UserId = userId,
                    FilePath = relativePath,
                    StoragePath = storagePath
                };

                _db.UploadedFiles.Add(uploadedFile);

                // Generate public URL
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                    var publicUrl = $"{baseUrl}/{relativePath}";
                    publicUrls.Add(publicUrl);
                }
            }

            await _db.SaveChangesAsync();

            return publicUrls; 
        }
    }
}
