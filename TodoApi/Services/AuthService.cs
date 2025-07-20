using System.IdentityModel.Tokens.Jwt; // Used for creating and handling JWT tokens
using System.Security.Claims; // Used for setting identity claims in JWT
using System.Text; // For encoding the secret key
using Microsoft.IdentityModel.Tokens; // For token encryption/signing
using TodoApi.Models; // Using the user and request models

namespace TodoApi.Services;

// Service responsible for user authentication logic: signup, login, JWT token generation
public class AuthService
{
    private readonly TaskDbService _db;
    private readonly IConfiguration _config;

    // Constructor injects database service and config (for secret key access)
    public AuthService(TaskDbService db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // Handles user registration
    public bool SignUp(RegisterRequest request)
    {
        // Check if user already exists (by email)
        if (_db.GetUserByEmail(request.Email) != null)
            return false;

        // Hash the password before storing it
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Save new user to the database
        _db.CreateUser(new User
        {
            Email = request.Email,
            Username = request.Username,
            Password = hashedPassword
        });

        return true; // Registration successful
    }

    // Validates user credentials during login
    public User? ValidateUser(LoginRequest request)
    {
        var user = _db.GetUserByEmail(request.Email);

        // Check if user exists and password is correct
        return user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.Password) ? user : null;
    }

    // Generates a JWT token for a successfully authenticated user
    public string GenerateToken(User user)
    {
        // Define user claims (data stored in the token)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim("email", user.Email),
            new Claim("username", user.Username)
        };

        // Create signing key from secret stored in appsettings
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the token object with claims, expiration, and signing credentials
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
            signingCredentials: creds
        );

        // Return the token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
