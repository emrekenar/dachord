namespace Domain.Models;

public class Chord
{
    public string Signature { get; set; } = string.Empty;
    public List<Note>? Notes { get; set; }
}