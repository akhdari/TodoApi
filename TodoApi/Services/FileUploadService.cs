using TodoApi.Models;
using TodoApi.Data;
using Microsoft.AspNetCore.Hosting;

namespace TodoApi.Services
{
    public class FileUploadService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public FileUploadService(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task SaveUploadedFiles(List<IFormFile> files, int userId)
        {
            // Fallback in case WebRootPath is not set
            //  the directory where public static files  are stored
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (_env.WebRootPath == null)
            {
                Console.WriteLine("Warning: WebRootPath is null. Using fallback path: " + rootPath);
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var fileName = Path.GetFileName(file.FileName);
                var userFolder = Path.Combine(rootPath, "uploads", userId.ToString());
                var storagePath = Path.Combine(userFolder, fileName);

                Directory.CreateDirectory(userFolder); // ensure directory exists

                using (var stream = new FileStream(storagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var uploadedFile = new UploadedFile
                {
                    UserId = userId,
                    FilePath = Path.Combine("uploads", userId.ToString(), fileName), // relative path for later use
                    StoragePath = storagePath // absolute path for internal reference
                };

                _db.UploadedFiles.Add(uploadedFile);
            }

            await _db.SaveChangesAsync();
        }
    }
}
