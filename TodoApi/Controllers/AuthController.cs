using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var registered = await _authService.SignUpAsync(request);
            if (!registered)
                return BadRequest(new { message = "User already exists" });

            var user = await _authService.ValidateUserAsync(new LoginRequest
            {
                Email = request.Email,
                Password = request.Password
            });

            if (user == null)
                return StatusCode(500, "Unexpected error");

            var token = _authService.GenerateToken(user);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var shareUrl = $"{baseUrl}/share/{user.Id}";

            return Ok(new { token, shareUrl });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.ValidateUserAsync(request);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = _authService.GenerateToken(user);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var shareUrl = $"{baseUrl}/share/{user.Id}";

            return Ok(new { token, shareUrl });
        }
    }
}
