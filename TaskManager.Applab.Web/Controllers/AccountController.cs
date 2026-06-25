using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TaskManager.Applab.Web.Models;
using System.IdentityModel.Tokens.Jwt;

namespace TaskManager.Applab.Web.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TaskApi");
    }

    // GET /Account/Login
    public IActionResult Login() => View();

    // POST /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input" });

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
        {
            model.Email,
            model.Password
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Invalid email or password" });


        var token = result.Data!;
        HttpContext.Session.SetString("JwtToken", token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
                       ?? model.Email;

        HttpContext.Session.SetString("Username", username);



        return Ok(new { redirectUrl = Url.Action("Index", "Task") });
    }

    // GET /Account/Register
    public IActionResult Register() => View();

    // POST /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input" });

        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
        {
            model.Username,
            model.Email,
            model.Password
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Registration failed" });

        var token = result.Data!;
        HttpContext.Session.SetString("JwtToken", token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
                       ?? model.Username;

        HttpContext.Session.SetString("Username", username);

        return Ok(new { redirectUrl = Url.Action("Index", "Task") });
    }

    // GET /Account/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }



    // GET /Account/ForgotPassword
    public IActionResult ForgotPassword() => View();

    // POST /Account/ForgotPassword
    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input" });

        var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { model.Email });
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        // The API always returns the same generic message — pass it straight through.
        return Ok(new { message = result?.Message ?? "If that email is registered, a password reset link has been sent." });
    }

    // GET /Account/ResetPassword?token=...
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login");

        return View(new ResetPasswordViewModel { Token = token });
    }

    // POST /Account/ResetPassword
    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input" });

        var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new
        {
            model.Token,
            model.NewPassword
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Could not reset password." });

        return Ok(new { redirectUrl = Url.Action("Login", "Account") });
    }


}


public record TokenResponse(string Token);