namespace Application.Interfaces;

using Domain.Wrappers;
using Application.Requests;
using Application.Responses;

public interface ILoginService
{
    Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request);
}