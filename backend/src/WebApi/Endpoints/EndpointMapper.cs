namespace WebApi.Endpoints;

using Application.Interfaces;
using Application.Requests;
using Domain.Interfaces;
using System.Security.Claims;

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

        app.MapGet("/user/me", async (HttpContext ctx, IUserRepository userRepository) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var user = await userRepository.GetByIdAsync(userId);
            return user is not null
                ? Results.Ok(new { displayName = user.DisplayName, role = user.Role.ToString() })
                : Results.NotFound();
        })
        .RequireAuthorization();

        app.MapPut("/user/display-name", async (UpdateDisplayNameRequest request, HttpContext ctx, IUpdateDisplayNameService service) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            await service.ExecuteAsync(userId, request.DisplayName);
            return Results.Ok();
        })
        .RequireAuthorization();
    }

    private static void MapTrackEndpoints(WebApplication app)
    {
        app.MapPost("/search", async (TrackSearchRequest request, ISearchChordsService searchChordsService) =>
        {
            var result = await searchChordsService.ExecuteAsync(request);
            return result is not null && result.Results.Count > 0 ? Results.Ok(result) : Results.NotFound();
        });

        app.MapGet("/track/{id}", async (string id, ISearchTracksService searchTracksService) =>
        {
            var track = await searchTracksService.GetTrackAsync(id);
            return track is not null ? Results.Ok(track) : Results.NotFound();
        });

        app.MapGet("/artist/{artistId}/albums", async (string artistId, ISearchTracksService searchTracksService) =>
        {
            var albums = await searchTracksService.GetArtistAlbumsAsync(artistId);
            return albums.Count > 0 ? Results.Ok(albums) : Results.NotFound();
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

        app.MapPost("/searchTracks", async (TrackSearchRequest request, ISearchTracksService searchTracksService) =>
        {
            var result = await searchTracksService.QueryAsync(request);
            return result.Results is null ? Results.BadRequest()
                : result.Results.Count == 0 ? Results.NotFound()
                : Results.Ok(result);
        });

        app.MapGet("/tracks/{trackId}/chords", async (string trackId, ITrackRepository trackRepository) =>
        {
            var versions = await trackRepository.GetTrackVersionsAsync(trackId);
            return Results.Ok(versions);
        });

        app.MapGet("/tracks/{trackId}/lyrics", async (string trackId, IGetLyricsService getLyricsService) =>
        {
            var result = await getLyricsService.ExecuteAsync(trackId);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .RequireAuthorization();

        app.MapPost("/chords/{trackId}/approve", async (string trackId, HttpContext ctx, IApproveChordService approveService) =>
        {
            var result = await approveService.ExecuteAsync(trackId);
            return result.IsSuccess ? Results.Ok() : Results.NotFound(result.Error);
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
    }
}