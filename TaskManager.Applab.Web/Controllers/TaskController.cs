using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Applab.Web.Models;

namespace TaskManager.Applab.Web.Controllers;

public class TaskController : Controller
{
    private readonly HttpClient _httpClient;

    public TaskController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TaskApi");
    }

    private void AttachToken()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private IActionResult RedirectIfNotLoggedIn()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");
        return null!;
    }

    // GET /Task
    public async Task<IActionResult> Index()
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<TaskViewModel>>>("api/task");
        var tasks = response?.Data ?? new List<TaskViewModel>();
        return View(tasks);
    }

    // POST /Task/Create  
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskViewModel model)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return BadRequest(new { message = "Not logged in" });

        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input" });

        AttachToken();
        var response = await _httpClient.PostAsJsonAsync("api/task", new
        {
            model.Title,
            model.Description,
            model.DueDate
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskViewModel>>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Failed to create task" });

        return Ok(result.Data);
    }

    // POST /Task/Edit  
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] EditTaskRequest request)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return BadRequest(new { message = "Not logged in" });

        AttachToken();
        var response = await _httpClient.PutAsJsonAsync($"api/task/{request.Id}", new
        {
            request.Title,
            request.Description,
            request.DueDate,
            Status = request.Status
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskViewModel>>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Failed to update task" });

        return Ok(result.Data);
    }

    // POST /Task/Delete/id
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        var response = await _httpClient.DeleteAsync($"api/task/{id}");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

        if (!response.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Failed to delete task" });

        return Ok();
    }

    // POST /Task/UpdateStatus
    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        var getResponse = await _httpClient.GetFromJsonAsync<ApiResponse<TaskViewModel>>($"api/task/{request.Id}");
        var task = getResponse?.Data;
        if (task == null) return NotFound();

        var putResponse = await _httpClient.PutAsJsonAsync($"api/task/{request.Id}", new
        {
            task.Title,
            task.Description,
            task.DueDate,
            Status = request.Status
        });

        var result = await putResponse.Content.ReadFromJsonAsync<ApiResponse<TaskViewModel>>();

        if (!putResponse.IsSuccessStatusCode || result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Failed to update status" });

        return Ok();
    }

    //Get /Search
    [HttpGet]
    public async Task<IActionResult> Search(string? search)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return Unauthorized();

        AttachToken();
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<TaskViewModel>>>("api/task");
        var tasks = response?.Data ?? new List<TaskViewModel>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            tasks = tasks.Where(t =>
                  t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                  t.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return Ok(tasks);
    }
}

public record UpdateStatusRequest(int Id, string Status);
public record EditTaskRequest(int Id, string Title, string Description, DateTime DueDate, string Status);