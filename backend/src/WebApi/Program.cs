using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Domain.Interfaces;
using Application.Requests;
using Application.Interfaces;
using Application.Services;
using Application.Configuration;
using Infrastructure;
using Infrastructure.External;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection(SpotifyOptions.SectionName));
builder.Services.Configure<MusicTheoryOptions>(builder.Configuration.GetSection(MusicTheoryOptions.SectionName));
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

app.MapGet("/tracks/{id}", async (string id, ITrackRepository trackRepository) =>
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
    var result = await submitChordsService.ExecuteAsync(track);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
})
.RequireAuthorization();

app.MapPost("/register", async (RegisterRequest request, IRegisterService registerService) =>
{
    var result = await registerService.ExecuteAsync(request);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});

app.MapPost("/login", async (LoginRequest request, ILoginService loginService) =>
{
    var result = await loginService.ExecuteAsync(request);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});

app.MapPost("/search", async (TrackSearchRequest request, ISearchTracksService trackSearchService) =>
{
    var result = await trackSearchService.ExecuteAsync(request);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});

app.Run();
