namespace Application.Interfaces;

public interface IUpdateDisplayNameService
{
    Task<bool> ExecuteAsync(string userId, string displayName);
}
