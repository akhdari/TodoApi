using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

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
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!_authService.SignUp(request))
            return BadRequest("User already exists");

        var user = _authService.ValidateUser(new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        });

        if (user == null) return StatusCode(500, "Unexpected error");

        var token = _authService.GenerateToken(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _authService.ValidateUser(request);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = _authService.GenerateToken(user);
        return Ok(new { token });
    }
}
