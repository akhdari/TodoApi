namespace TodoApi.Models
{
    public class UploadResult
    {
        public string Token { get; set; } = string.Empty;
        public List<string> SkippedFiles { get; set; } = new();
    }
}
