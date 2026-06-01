namespace Application.Responses;

using Domain.Models.Track;

public record LyricsResponse(List<Section> Sections);
