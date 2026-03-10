using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Domain.Models;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Application.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITrackRepository, TrackRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Set to true and provide 'ValidIssuer' in production
        ValidateAudience = false, // Set to true and provide 'ValidAudience' in production
        ClockSkew = TimeSpan.Zero // Immediate expiration
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapGet("/tracks/{id}", async (int id, ITrackRepository trackRepository) =>
{
    var track = await trackRepository.GetByIdAsync(id);
    return track is not null ? Results.Ok(track) : Results.NotFound();
});

app.MapGet("/tracks", async (ITrackRepository trackRepository) =>
{
    var tracks = await trackRepository.GetAllAsync();
    return Results.Ok(tracks);
});

app.MapPost("/tracks", async (TrackDto track, ITrackRepository trackRepository) =>
{
    var trackModel = new Track
    {
        Title = track.Title,
        Artist = track.Artist,
        Lyrics = track.Lyrics
    };
    await trackRepository.SaveAsync(trackModel);
    return Results.Created($"/tracks/{trackModel.Id}", trackModel);
})
.RequireAuthorization();

app.MapPost("/register", async (RegisterDto dto, IUserRepository userRepository) =>
{
    var existingUser = await userRepository.GetByEmailAsync(dto.Email);
    if (existingUser is not null)
    {
        return Results.BadRequest("User already exists.");
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
    var user = new User
    {
        Email = dto.Email,
        PasswordHash = passwordHash
    };
    await userRepository.CreateUserAsync(user);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPost("/login", async (LoginDto dto, IUserRepository userRepository) =>
{
    var user = await userRepository.GetByEmailAsync(dto.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    // var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
    // var key = Encoding.ASCII.GetBytes(jwtKey);

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
        }),
        Expires = DateTime.UtcNow.AddMinutes(double.Parse(builder.Configuration["Jwt:ExpireMinutes"] ?? "60")),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = tokenString });
});

app.Run();
