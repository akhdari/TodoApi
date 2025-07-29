using System.Runtime.Serialization.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly FileUploadService _uploadService;
    private readonly long _maxFileSize;

    public UploadsController(FileUploadService uploadService, IConfiguration _config)
    {
        // Initialize the upload service
        _uploadService = uploadService;
        // Get the max file size from configuration
        _maxFileSize = _config.GetValue<long>("UploadSettings:MaxSize");
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        if (files.Any(f => f.Length == 0))
            return BadRequest("One or more files are empty");

        if (files.Any(f => f.Length > _maxFileSize))
            return BadRequest("One or more files exceed the size limit of 10 MB");

        int userId = GetUserIdFromClaims();

        // ✅ Capture the returned URLs
        var urls = await _uploadService.SaveUploadedFiles(files, userId);

        // ✅ Return the message + URLs
        return Ok(new
        {
            message = "Files uploaded successfully.",
            userId = userId,
            urls = urls
        });
    }



    private int GetUserIdFromClaims()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type.EndsWith("nameidentifier"));
        if (claim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(claim.Value);
    }

}
