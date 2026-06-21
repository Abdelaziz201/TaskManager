using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Applab.Application.Common;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Application.Settings;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Services;

public class AuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IAuthRepository authRepository, IOptions<JwtSettings> jwtSettings)
    {
        _authRepository = authRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterDto dto)
    {
        if (await _authRepository.EmailExistsAsync(dto.Email))
            return ApiResponse<string>.Fail("Email already exists");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _authRepository.AddUserAsync(user);
        var token = GenerateToken(user);

        return ApiResponse<string>.Ok(token, "Registration successful");
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginDto dto)
    {
        var user = await _authRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ApiResponse<string>.Fail("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<string>.Fail("Invalid email or password");

        var token = GenerateToken(user);
        return ApiResponse<string>.Ok(token, "Login successful");
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}