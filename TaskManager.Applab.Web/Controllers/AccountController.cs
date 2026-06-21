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
}

public record TokenResponse(string Token);