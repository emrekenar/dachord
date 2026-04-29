namespace Application.Interfaces;

public interface IChordService
{
    Task<List<string>> GetAvailableChords();
}