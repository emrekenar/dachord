namespace Application.Interfaces;

using Domain.Models;
using Domain.Wrappers;
using Application.Requests;

public interface IRegisterService
{
    Task<Result<User>> ExecuteAsync(RegisterRequest request);
}