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
            Console.WriteLine($"Created batch with Id={batch.Id}, Token={batch.Token}");
            return batch;
        }
        public async Task<string?> GetTokenByFilePathAsync(string filePath)
        {
            var file = await _db.UploadedFiles
                        .Include(f => f.UploadBatch)
                        .FirstOrDefaultAsync(f => f.FilePath == filePath);
            return file?.UploadBatch?.Token;
        }

        public async Task AddUploadedFilesAsync(List<UploadedFile> files)
        {
            _db.UploadedFiles.AddRange(files);
            await _db.SaveChangesAsync();
        }

        public async Task<List<string>> GetFilePathsByTokenAsync(string token)
        {
            var batch = await _db.UploadBatches
                .Include(b => b.Files)
                .FirstOrDefaultAsync(b => b.Token == token);

            if (batch == null || batch.Files == null || !batch.Files.Any())
                return new List<string>();

            return batch.Files.Select(f => f.FilePath).ToList();
        }


    }
}
