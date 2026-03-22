namespace Application.Services;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using Domain.Interfaces;
using Domain.Errors;
using Domain.Wrappers;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Application.Configuration;

public class LoginService(IOptions<JwtOptions> jwtOptions, IUserRepository userRepository) : ILoginService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(new Error(ErrorCode.InvalidCredentials, "Invalid email or password."));
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
        return Result<LoginResponse>.Success(responseValue);
    }
}