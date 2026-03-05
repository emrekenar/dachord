using Domain.Models;
using Domain.Interfaces;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITrackRepository, TrackRepository>();

var app = builder.Build();

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

app.MapPost("/tracks", async (Track track, ITrackRepository trackRepository) =>
{
    await trackRepository.SaveAsync(track);
    return Results.Created($"/tracks/{track.Id}", track);
});

app.Run();
