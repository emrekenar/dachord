namespace Application.Interfaces;

using Application.Models;

public interface ILoginService
{
    Task<IResult> ExecuteAsync(LoginRequest request);
}