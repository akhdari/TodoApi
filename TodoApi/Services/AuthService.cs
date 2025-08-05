using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class AuthService
    {
        private readonly UserDbService _db;
        private readonly IConfiguration _config;

        public AuthService(UserDbService db, IConfiguration config)
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
        // Generate password reset token
        public string GeneratePasswordResetToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("email", user.Email)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // short-lived
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        // Request password reset
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _db.GetUserByEmailAsync(email);
            if (user == null)
                return true; // Always return true for security

            var resetToken = GeneratePasswordResetToken(user);


            Console.WriteLine("==== Password Reset Request ====");
            Console.WriteLine("Use this token in /auth/reset-password:");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Token: {resetToken}");
            Console.WriteLine("================================");


            return true;
        }
        // Reset password using token
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // no extra time buffer
                }, out SecurityToken validatedToken);

                var tokenEmail = principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                if (tokenEmail == null || tokenEmail != email)
                    return false;

                var user = await _db.GetUserByEmailAsync(email);
                if (user == null)
                    return false;

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _db.UpdateUserAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
