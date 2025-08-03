using TodoApi.Models;

using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class UploadBatch
{
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = Guid.NewGuid().ToString("N"); // unique token for shareable link

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UploadedFile> Files { get; set; } = new();
}
