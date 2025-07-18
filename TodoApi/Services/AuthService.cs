using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;
using BCrypt.Net;
using TodoApi;

namespace TodoApi.Services;

public class AuthService
{
    private readonly TaskDbService _db;

    public AuthService(TaskDbService db)
    {
        _db = db;
    }

    public bool SignUp(RegisterRequest request)
    {
        if (_db.GetUserByEmail(request.Email) != null)
            return false;

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        _db.CreateUser(new User
        {
            Email = request.Email,
            Username = request.Username,
            Password = hashedPassword
        });

        return true;
    }

    public User? ValidateUser(LoginRequest request)
    {
        var user = _db.GetUserByEmail(request.Email);
        return user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.Password) ? user : null;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim("email", user.Email),
            new Claim("username", user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConstants.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
