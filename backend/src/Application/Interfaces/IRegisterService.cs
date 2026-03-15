namespace Application.Interfaces;

using Application.Models;

public interface IRegisterService
{
    Task<IResult> ExecuteAsync(RegisterRequest request);
}