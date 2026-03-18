namespace Application.Requests;

using Domain.Models;

public class SubmitChordsRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public Lyrics? Lyrics { get; set; }
}