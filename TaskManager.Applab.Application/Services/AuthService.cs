using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private const string ResetTokenAudience = "password-reset";
    private const int ResetTokenExpiryMinutes = 30;

    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly FrontendSettings _frontendSettings;

    public AuthService(IAuthRepository authRepository,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<FrontendSettings> frontendSettings)
    {
        _authRepository = authRepository;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
        _frontendSettings = frontendSettings.Value;
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


    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _authRepository.GetByEmailAsync(dto.Email);

        
        const string genericMessage = "If that email is registered, a password reset link has been sent.";

        if (user == null)
            return ApiResponse<string>.Ok(string.Empty, genericMessage);

        var token = GenerateResetToken(user);
        var resetLink = $"{_frontendSettings.BaseUrl.TrimEnd('/')}/Account/ResetPassword?token={Uri.EscapeDataString(token)}";

        await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

        return ApiResponse<string>.Ok(string.Empty, genericMessage);
    }


    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var principal = ValidateResetToken(dto.Token);
        if (principal == null)
            return ApiResponse<string>.Fail("This reset link is invalid or has expired. Please request a new one.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return ApiResponse<string>.Fail("This reset link is invalid or has expired. Please request a new one.");

        var user = await _authRepository.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<string>.Fail("This reset link is invalid or has expired. Please request a new one.");

        var tokenFingerprint = principal.FindFirst("pwdFingerprint")?.Value;
        var currentFingerprint = ComputeFingerprint(user.PasswordHash);

        if (tokenFingerprint != currentFingerprint)
            return ApiResponse<string>.Fail("This reset link has already been used or your password has changed. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _authRepository.UpdateUserAsync(user);

        return ApiResponse<string>.Ok(string.Empty, "Your password has been reset. You can now log in.");
    }



    private string GenerateResetToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("pwdFingerprint", ComputeFingerprint(user.PasswordHash))
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: ResetTokenAudience,        
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateResetToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = ResetTokenAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            // Expired, tampered, wrong audience, malformed — all treated the same: invalid link.
            return null;
        }
    }

    private static string ComputeFingerprint(string passwordHash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash));
        return Convert.ToBase64String(bytes);
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