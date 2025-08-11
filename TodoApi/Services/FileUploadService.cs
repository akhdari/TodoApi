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

    var skippedFiles = new List<string>();
    var uploadedFiles = new List<UploadedFile>();

    // Collect existing batch tokens for skipped files
    var existingTokens = new HashSet<string>();

    foreach (var file in files)
    {
        if (file == null || file.Length == 0 || file.Length > maxSize)
        {
            skippedFiles.Add(file?.FileName ?? "Unknown");
            continue;
        }

        var fileName = Path.GetFileName(file.FileName);
        var relativePath = $"uploads/{userId}/{fileName}";

        // Check if file already uploaded, get its batch token
        var existingToken = await _fileDb.GetTokenByFilePathAsync(relativePath);
        if (!string.IsNullOrEmpty(existingToken))
        {
            skippedFiles.Add(fileName);
            existingTokens.Add(existingToken);
            continue; // Skip file upload
        }

        // Prepare physical file saving
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
            // UploadBatchId will be set after batch creation
        });
    }

    // Determine batch token to return
    if (uploadedFiles.Count == 0)
    {
        // All files skipped, if they belong to same batch return that batch token
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
            // No files uploaded and no single batch, create new batch anyway
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
        // New files uploaded, create batch and assign
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
    }
}
