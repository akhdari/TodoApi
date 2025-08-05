using TodoApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TodoApi.Services.Db;

namespace TodoApi.Services
{
    public class FileUploadService
    {
        private readonly FileDbService _fileDb;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public FileUploadService(
            FileDbService fileDb,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _fileDb = fileDb;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<UploadResult> SaveUploadedFiles(List<IFormFile> files, int userId)
        {
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var maxSize = _configuration.GetValue<long>("UploadSettings:MaxSize");

            var batch = await _fileDb.CreateBatchAsync();

            var result = new UploadResult
            {
                Token = batch.Token,
                SkippedFiles = new List<string>()
            };

            var uploadedFiles = new List<UploadedFile>();

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

                uploadedFiles.Add(new UploadedFile
                {
                    UserId = userId,
                    FilePath = relativePath,
                    StoragePath = storagePath,
                    UploadBatchId = batch.Id
                });
            }

            await _fileDb.AddUploadedFilesAsync(uploadedFiles);
            return result;
        }

        public async Task<List<string>> GetSharedFileUrls(string token)
        {
            var paths = await _fileDb.GetFilePathsByTokenAsync(token);

            var httpContext = _httpContextAccessor.HttpContext;
            var baseUrl = httpContext != null ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" : "";

            return paths.Select(path => $"{baseUrl}/{path}").ToList();
        }
    }
}
