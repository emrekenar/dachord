using Domain.Models;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Application.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITrackRepository, TrackRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

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
});

app.Run();
