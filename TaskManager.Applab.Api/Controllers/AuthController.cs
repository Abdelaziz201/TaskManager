using Microsoft.AspNetCore.Mvc;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Services;

namespace TaskManager.Applab.Api.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly AuthService _authservice;

        public AuthController(AuthService authService)
        {
            _authservice = authService;
        }

        //post /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
        var result = await _authservice.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

        //post /api/auth/login
        [HttpPost("login")]
        public async Task <IActionResult> Login([FromBody] LoginDto dto)
        {
        var result = await _authservice.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    //post /api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authservice.ForgotPasswordAsync(dto);
        return Ok(result); // always 200 — the message is identical whether or not the email exists
    }

    //post /api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authservice.ResetPasswordAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
