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
            // WebRootPath = the path to the www root folder on the server
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var maxSize = _configuration.GetValue<long>("UploadSettings:MaxSize");

            var skippedFiles = new List<string>();
            var uploadedFiles = new List<UploadedFile>();

            var existingTokens = new HashSet<string>();//data structure that stores unique elements only and allows for fast lookups

            foreach (var file in files)
            {
                if (file == null || file.Length == 0 || file.Length > maxSize)
                {
                    skippedFiles.Add(file?.FileName ?? "Unknown");
                    continue;
                }

                var fileName = Path.GetFileName(file.FileName);
                var relativePath = $"uploads/{userId}/{fileName}";

                var existingToken = await _fileDb.GetTokenByFilePathAsync(relativePath);
                if (!string.IsNullOrEmpty(existingToken))
                {
                    skippedFiles.Add(fileName);
                    existingTokens.Add(existingToken);
                    continue;
                }

                var userFolder = Path.Combine(rootPath, "uploads", userId.ToString());
                Directory.CreateDirectory(userFolder);

                var storagePath = Path.Combine(userFolder, fileName);

                if (File.Exists(storagePath))
                {
                    skippedFiles.Add(fileName);
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
                    StoragePath = storagePath
                });
            }

            if (uploadedFiles.Count == 0)
            {
                if (existingTokens.Count == 1)
                {
                    return new UploadResult
                    {
                        Token = existingTokens.First(),
                        SkippedFiles = skippedFiles
                    };
                }
                else
                {
                    var newBatch = await _fileDb.CreateBatchAsync();
                    return new UploadResult
                    {
                        Token = newBatch.Token,
                        SkippedFiles = skippedFiles
                    };
                }
            }
            else
            {
                var batch = await _fileDb.CreateBatchAsync();

                foreach (var uf in uploadedFiles)
                {
                    uf.UploadBatchId = batch.Id;
                }

                await _fileDb.AddUploadedFilesAsync(uploadedFiles);

                return new UploadResult
                {
                    Token = batch.Token,
                    SkippedFiles = skippedFiles
                };
            }
        }

        public async Task<List<string>> GetSharedFileUrls(string token)
        {
            var paths = await _fileDb.GetFilePathsByTokenAsync(token);

            var httpContext = _httpContextAccessor.HttpContext;
            var baseUrl = httpContext != null ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" : "";

            return paths.Select(path => $"{baseUrl}/{path}").ToList();
        }

        public async Task<FileStreamResultData?> GetFileStreamForDownload(string token, string fileName)
        {
            var paths = await _fileDb.GetFilePathsByTokenAsync(token);
            var match = paths.FirstOrDefault(p => Path.GetFileName(p).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return null;

            var physicalPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), match);

            if (!File.Exists(physicalPath))
                return null;

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentType(fileName);

            return new FileStreamResultData
            {
                Stream = stream,
                ContentType = contentType,
                FileName = fileName
            };
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }

    public class FileStreamResultData
    {
        public required Stream Stream { get; set; }
        public required string ContentType { get; set; }
        public required string FileName { get; set; }
    }


}
