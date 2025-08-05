using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TodoApi.Services;
using TodoApi.Services.Db;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();


// Midlleware to handle exceptions globally
// (Moved below after 'app' is declared)
// Configure Swagger with JWT auth
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Description = "Enter 'Bearer' followed by the token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

// Custom services
builder.Services.AddScoped<TaskDbService>();
builder.Services.AddScoped<UserDbService>();
builder.Services.AddScoped<FileDbService>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FileUploadService>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<ISieveProcessor, SieveProcessor>();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 4, 32))
    ));
// Read JWT secret key from config
var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new Exception("Secret key must be defined in appsettings.json and at least 256 bits (32 characters).");
}

// Configure JWT auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddTransient<TodoApi.Middlewares.GlobalExceptionHandling>();


var app = builder.Build();



// Middleware
// after 'app' is declared
app.Use( async(context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseStaticFiles(); //wwwroot/*
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TodoApi.Middlewares.GlobalExceptionHandling>();
app.MapControllers();
app.Run();
