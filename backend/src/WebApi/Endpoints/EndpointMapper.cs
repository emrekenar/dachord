namespace WebApi.Endpoints;

using Application.Interfaces;
using Application.Requests;
using Domain.Interfaces;

public static class EndpointMapper
{
    public static void MapEndpoints(WebApplication app)
    {
        MapUserEndpoints(app);
        MapTrackEndpoints(app);
        MapTestEndpoints(app); // check dev env
    }

    private static void MapUserEndpoints(WebApplication app)
    {
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
    }

    private static void MapTrackEndpoints(WebApplication app)
    {
        app.MapPost("/search", async (TrackSearchRequest request, ISearchChordsService searchChordsService) =>
        {
            var result = await searchChordsService.ExecuteAsync(request);
            return result is not null && result.Results.Count > 0 ? Results.Ok(result) : Results.NotFound();
        });

        app.MapGet("/chords/{id}", async (string id, ITrackRepository trackRepository) =>
        {
            var trackVersion = await trackRepository.GetTrackVersionAsync(id);
            return trackVersion is not null ? Results.Ok(trackVersion) : Results.NotFound();
        });

        app.MapPost("/tracks", async (SubmitChordsRequest request, ISubmitChordsService submitChordsService) =>
        {
            var result = await submitChordsService.ExecuteAsync(request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .RequireAuthorization();
    }

    private static void MapTestEndpoints(WebApplication app)
    {
        app.MapGet("/tracks/{id}", async (string id, ITrackRepository trackRepository) =>
        {
            var track = await trackRepository.GetTrackAsync(id);
            return track is not null ? Results.Ok(track) : Results.NotFound();
        });

        app.MapPost("/searchTracks", async (TrackSearchRequest request, ISearchTracksService searchTracksService) =>
        {
            var result = await searchTracksService.QueryAsync(request);
            return result.Results is null ? Results.BadRequest()
                : result.Results.Count == 0 ? Results.NotFound()
                : Results.Ok(result);
        });
    }
}