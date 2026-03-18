namespace Application.Interfaces;

using Microsoft.AspNetCore.Http.HttpResults;
using Application.Requests;
using Application.Responses;

public interface ILoginService
{
    Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> ExecuteAsync(LoginRequest request);
}