using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class AuthService
    {
        private readonly TaskDbService _db;
        private readonly IConfiguration _config;

        public AuthService(TaskDbService db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // Register new user
        public async Task<bool> SignUpAsync(RegisterRequest request)
        {
            var existingUser = await _db.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                return false;
                

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Email = request.Email,
                Username = request.Username,
                Password = hashedPassword
            };

            await _db.CreateUserAsync(newUser);
            return true;
        }

        // Validate user login
        public async Task<User?> ValidateUserAsync(LoginRequest request)
        {
            var user = await _db.GetUserByEmailAsync(request.Email);
            if (user == null) return null;

            return BCrypt.Net.BCrypt.Verify(request.Password, user.Password) ? user : null;
        }

        // Generate JWT
        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("username", user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
