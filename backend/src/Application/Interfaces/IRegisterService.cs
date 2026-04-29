namespace Application.Interfaces;

using Domain.Models.User;
using Domain.Wrappers;
using Application.Requests;

public interface IRegisterService
{
    Task<Result<User>> ExecuteAsync(RegisterRequest request);
}