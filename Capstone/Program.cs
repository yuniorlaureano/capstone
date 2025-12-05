using Microsoft.EntityFrameworkCore;
using Capstone;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CapstoneDb>(implementationInstance: new CapstoneDb("Data Source=app.db;Version=3;"));
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

var key = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));
    options.AddPolicy("Requireguest", policy => policy.RequireRole("guest"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Example: only admins can access
app.MapPost("/api/register", async ([FromBody] UserDto model, CapstoneDb db) =>
{
    if (!ValidationHelper.IsValidInput(model.Password, "!@#$%^&*()-_=+[]{};:,.<>?") || !ValidationHelper.IsValidInput(model.Email, "@."))
    {
        return Results.BadRequest("Invalid input.");
    }

    if (!ValidationHelper.IsValidXSSInput(model.UserName) || !ValidationHelper.IsValidXSSInput(model.Password) || !ValidationHelper.IsValidXSSInput(model.Email))
    {
        return Results.BadRequest("Invalid input");
    }

    var existingUser = await db.GetUserByUsernameAsync(model.UserName);
    if (existingUser != null)
    {
        return Results.Conflict("Username already exists.");
    }

    var user = new User();
    user.UserName = model.UserName;
    user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
    user.Email = model.Email;
    user.Role = model.Role ?? "guest"; // default role

    var createdUser = await db.RegisterUserAsync(user);
    return Results.Ok(new { createdUser.Id, createdUser.UserName });
});

app.MapGet("/api/login", async ([FromBody] LoginDto model, CapstoneDb db) =>
{
    if (!ValidationHelper.IsValidXSSInput(model.UserName) || !ValidationHelper.IsValidXSSInput(model.Password))
    {
        return Results.BadRequest("Invalid input");
    }

    var user = await db.GetUserByUsernameAsync(model.UserName);
    if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: null,
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256)
    );  
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(tokenString);
});

// Example: only admins can access
app.MapGet("/api/users", async (CapstoneDb db) =>
{
    var users = await db.GetUsersAsync();
    return users.Select(u => new { u.Id, u.UserName, u.Email });
})
.RequireAuthorization("RequireAdmin");

app.MapPut("/api/users/{id}/role", async ([FromBody] RoleAssignDto model, [FromRoute] Guid id, CapstoneDb db) =>
{
    if (model.Role != "admin" && model.Role != "guest")
    {
        return Results.BadRequest("Invalid input");
    }
    
    await db.AssignRoleAsync(id, model.Role);
    return Results.Ok();
}).RequireAuthorization("RequireAdmin");

// Example: guest role can access a different endpoint
app.MapGet("/api/general", () => "Welcome, guest User!")
   .RequireAuthorization("Requireguest");

app.Run();
