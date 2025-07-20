namespace TodoApi.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
/*
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Tools

create model
add db context
Registered AppDbContext in Program.cs
Set your database provider 
create migration
apply migration
*/