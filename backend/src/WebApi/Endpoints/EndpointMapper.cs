namespace WebApi.Endpoints;

using Application.Interfaces;
using Application.Requests;
using Domain.Interfaces;

public static class EndpointMapper
{
    public static void MapEndpoints(WebApplication app)
    {
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
    }
}