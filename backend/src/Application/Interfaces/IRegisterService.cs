namespace Application.Interfaces;

using Microsoft.AspNetCore.Http;
using Application.Requests;

public interface IRegisterService
{
    Task<IResult> ExecuteAsync(RegisterRequest request);
}