using TodoApi.Data;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Services.Db
{
    public class FileDbService
    {
        private readonly AppDbContext _db;

        public FileDbService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UploadBatch> CreateBatchAsync()
        {
            var batch = new UploadBatch
            {
                Token = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            _db.UploadBatches.Add(batch);
            await _db.SaveChangesAsync();
            return batch;
        }

        public async Task AddUploadedFilesAsync(List<UploadedFile> files)
        {
            _db.UploadedFiles.AddRange(files);
            await _db.SaveChangesAsync();
        }

        public async Task<List<string>> GetFilePathsByTokenAsync(string token)
        {
            return await _db.UploadBatches
                .Where(b => b.Token == token)
                .SelectMany(b => b.Files.Select(f => f.FilePath))
                .ToListAsync();
        }
    }
}
