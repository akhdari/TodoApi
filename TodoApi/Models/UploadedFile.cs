using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;
//
public class UploadedFile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;     

    [Required]
    public string StoragePath { get; set; } = string.Empty;   
    public DateTime UploadTime { get; init; } = DateTime.UtcNow;
}
