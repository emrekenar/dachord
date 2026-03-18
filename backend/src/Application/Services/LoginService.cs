namespace Application.Services;

using Microsoft.AspNetCore.Http;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Application.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;

public class LoginService(IOptions<JwtOptions> jwtOptions, IUserRepository userRepository) : ILoginService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> ExecuteAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return TypedResults.Unauthorized();
        }

        var key = Encoding.ASCII.GetBytes(_jwtOptions.Key);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var responseValue = new LoginResponse { Token = tokenString };
        return TypedResults.Ok(responseValue);
    }
}