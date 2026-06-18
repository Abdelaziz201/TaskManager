using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TaskManager.Applab.Web.Models;

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

        if (!response.IsSuccessStatusCode)
            return BadRequest(new { message = "Invalid email or password" });

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        HttpContext.Session.SetString("JwtToken", result!.Token);
        HttpContext.Session.SetString("Username", model.Email);

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

        if (!response.IsSuccessStatusCode)
            return BadRequest(new { message = "Email already exists" });

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        HttpContext.Session.SetString("JwtToken", result!.Token);
        HttpContext.Session.SetString("Username", model.Username);

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