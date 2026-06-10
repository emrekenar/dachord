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
                ? Results.Ok(new
                {
                    displayName = user.DisplayName,
                    bio = user.Bio,
                    avatarIcon = user.AvatarIcon,
                    role = user.Role.ToString(),
                    numberOfApprovedSongs = user.NumberOfApprovedSongs,
                    numberOfLikes = user.NumberOfLikes,
                })
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

        app.MapPut("/user/profile", async (UpdateProfileRequest request, HttpContext ctx, IUserRepository userRepository) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            await userRepository.UpdateProfileAsync(userId, request.Bio, request.AvatarIcon);
            return Results.Ok();
        })
        .RequireAuthorization();

        // Public profile — non-sensitive fields only (no email).
        app.MapGet("/user/{id}", async (string id, IUserRepository userRepository) =>
        {
            var user = await userRepository.GetByIdAsync(id);
            return user is not null
                ? Results.Ok(new
                {
                    displayName = user.DisplayName,
                    bio = user.Bio,
                    avatarIcon = user.AvatarIcon,
                    role = user.Role.ToString(),
                    numberOfApprovedSongs = user.NumberOfApprovedSongs,
                    numberOfLikes = user.NumberOfLikes,
                })
                : Results.NotFound();
        });

        app.MapPost("/feedback", async (SubmitFeedbackRequest request, HttpContext ctx, IFeedbackService feedbackService) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await feedbackService.ExecuteAsync(userId, email, request.Message);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization();

        MapAdminEndpoints(app);
    }

    private static void MapAdminEndpoints(WebApplication app)
    {
        app.MapGet("/admin/users", async (IUserRepository userRepository) =>
        {
            var users = await userRepository.GetAllUsersAsync();
            return Results.Ok(users.Select(u => new
            {
                id = u.Id,
                email = u.Email,
                displayName = u.DisplayName,
                avatarIcon = u.AvatarIcon,
                role = u.Role.ToString(),
            }));
        })
        .RequireAuthorization("AdminOnly");

        app.MapPut("/admin/users/{userId}/role", async (string userId, UpdateRoleRequest request, IUserRepository userRepository) =>
        {
            if (!Enum.TryParse<Domain.Models.User.UserRole>(request.Role, ignoreCase: true, out var role))
                return Results.BadRequest("Invalid role.");
            await userRepository.UpdateRoleAsync(userId, role);
            return Results.Ok();
        })
        .RequireAuthorization("AdminOnly");
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

        app.MapPost("/chords/{trackId}/approve/{contributorId}", async (string trackId, string contributorId, IApproveChordService approveService) =>
        {
            var result = await approveService.ExecuteAsync(trackId, contributorId);
            return result.IsSuccess ? Results.Ok() : Results.NotFound(result.Error);
        })
        .RequireAuthorization("ModeratorOrAdmin");

        app.MapGet("/moderation/queue", async (IModerationQueueService moderationQueueService) =>
        {
            var items = await moderationQueueService.ExecuteAsync(minLikes: 2);
            return Results.Ok(items);
        })
        .RequireAuthorization("ModeratorOrAdmin");
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