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

    public UploadsController(FileUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0) //No input/ empty list
            return BadRequest("No files uploaded");

        int userId = GetUserIdFromClaims();
        await _uploadService.SaveUploadedFiles(files, userId);

        return Ok(new
        {
            Message = "Files uploaded successfully.",
            UserId = userId
        });
    }

    private int GetUserIdFromClaims()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type.EndsWith("nameidentifier"));
        if (claim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(claim.Value);
    }
}
