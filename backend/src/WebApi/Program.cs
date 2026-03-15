using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Domain.Interfaces;
using Infrastructure.Persistence;
using Application.Models;
using Infrastructure.Configuration;
using Application.Interfaces;
using Infrastructure.External;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection(SpotifyOptions.SectionName));

builder.Services.AddSingleton<ITrackRepository, TrackRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ISubmitChordsService, SubmitChordsService>();
builder.Services.AddHttpClient<ISearchTracksService, SpotifySearchTracksService>();

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

app.MapPost("/tracks", async (SubmitChordsRequest track, ISubmitChordsService submitChordsService) =>
{
    return await submitChordsService.ExecuteAsync(track);
})
.RequireAuthorization();

app.MapPost("/register", async (RegisterRequest request, IRegisterService registerService) =>
{
    return await registerService.ExecuteAsync(request);
});

app.MapPost("/login", async (LoginRequest request, ILoginService loginService) =>
{
    return await loginService.ExecuteAsync(request);
});

app.MapPost("/search", async (TrackSearchRequest request, ISearchTracksService trackSearchService) =>
{
    return await trackSearchService.ExecuteAsync(request);
});

app.Run();
