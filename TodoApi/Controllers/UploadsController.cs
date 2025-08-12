using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly FileUploadService _uploadService;
        private readonly long _maxFileSize;

        public UploadsController(FileUploadService uploadService, IConfiguration config)
        {
            _uploadService = uploadService;
            _maxFileSize = config.GetValue<long>("UploadSettings:MaxSize");
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            if (files.Any(f => f.Length == 0))
                return BadRequest("One or more files are empty");

            if (files.Any(f => f.Length > _maxFileSize))
                return BadRequest("One or more files exceed the size limit");

            int userId = GetUserIdFromClaims();

            var uploadResult = await _uploadService.SaveUploadedFiles(files, userId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var shareableUrl = $"{baseUrl}/uploads/shared/{uploadResult.Token}";

            return Ok(new
            {
                message = "Files uploaded successfully.",
                shareableUrl = shareableUrl,
                skippedfiles = uploadResult.SkippedFiles
            });
        }

        private int GetUserIdFromClaims()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type.EndsWith("nameidentifier"));
            if (claim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(claim.Value);
        }

        [HttpGet("shared/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedFiles(string token)
        {
            var urls = await _uploadService.GetSharedFileUrls(token);

            if (urls.Count == 0)
                return NotFound("Invalid or expired link");

            return Ok(new
            {
                files = urls
            });
        }

    }
}