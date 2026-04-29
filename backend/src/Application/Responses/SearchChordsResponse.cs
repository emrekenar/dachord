namespace Application.Responses;

public record TrackVersionsPair(TrackResponse Track, List<TrackVersionResponse> Versions);

public class SearchChordsResponse
{
    public List<TrackVersionsPair> Results { get; set; } = [];
}