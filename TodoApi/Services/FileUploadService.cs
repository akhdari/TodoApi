using TodoApi.Models;
using TodoApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Services
{
    public class FileUploadService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public FileUploadService(
            AppDbContext db,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _db = db;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<UploadResult> SaveUploadedFiles(List<IFormFile> files, int userId)
        {
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var maxSize = _configuration.GetValue<long>("UploadSettings:MaxSize");

            var batch = new UploadBatch
            {
                Token = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            _db.UploadBatches.Add(batch);
            await _db.SaveChangesAsync();

            var result = new UploadResult
            {
                Token = batch.Token,
                SkippedFiles = new List<string>()
            };

            foreach (var file in files)
            {
                if (file == null || file.Length == 0 || file.Length > maxSize)
                {
                    result.SkippedFiles.Add(file?.FileName ?? "Unknown");
                    continue;
                }

                var fileName = Path.GetFileName(file.FileName);
                var userFolder = Path.Combine(rootPath, "uploads", userId.ToString());
                Directory.CreateDirectory(userFolder);

                var storagePath = Path.Combine(userFolder, fileName);
                var relativePath = $"uploads/{userId}/{fileName}";

                if (File.Exists(storagePath))
                {
                    result.SkippedFiles.Add(fileName);
                    continue;
                }

                using (var stream = new FileStream(storagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var uploadedFile = new UploadedFile
                {
                    UserId = userId,
                    FilePath = relativePath,
                    StoragePath = storagePath,
                    UploadBatchId = batch.Id
                };

                _db.UploadedFiles.Add(uploadedFile);
            }

            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<List<string>> GetSharedFileUrls(string token)
        {
            var batch = await _db.UploadBatches
                .Where(b => b.Token == token)
                .Select(b => new
                {
                    b.CreatedAt,
                    Files = b.Files.Select(f => f.FilePath)
                })
                .FirstOrDefaultAsync();

            if (batch == null) return new List<string>();

            var httpContext = _httpContextAccessor.HttpContext;
            var baseUrl = httpContext != null ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" : "";

            return batch.Files.Select(path => $"{baseUrl}/{path}").ToList();
        }
    }
}
