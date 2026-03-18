using Domain.Models;

namespace Application.Responses;

public class TrackResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string Url { get; set; } = string.Empty;
    public Lyrics? Lyrics { get; set; }
}