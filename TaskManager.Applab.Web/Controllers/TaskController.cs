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

    //Get/Task
    public async Task<IActionResult> Index()
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        var tasks = await _httpClient.GetFromJsonAsync<List<TaskViewModel>>("api/task");
        return View(tasks ?? new List<TaskViewModel>());
    }

    //post/Task/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
            return RedirectToAction("Index");

        AttachToken();
        await _httpClient.PostAsJsonAsync("api/task", new
        {
            model.Title,
            model.Description,
            model.DueDate
        });

        return RedirectToAction("Index");

    }

    //Post /Task/Delete/id
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        await _httpClient.DeleteAsync($"api/task/{id}");
        return Ok();
    }

    //Post /Task/update
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        var redirect = RedirectIfNotLoggedIn();
        if (redirect != null) return redirect;

        AttachToken();
        var task = await _httpClient.GetFromJsonAsync<TaskViewModel>($"api/task/{request.Id}");
        if (task == null) return NotFound();

        await _httpClient.PutAsJsonAsync($"api/task/{request.Id}", new
        {
            task.Title,
            task.Description,
            task.DueDate,
            Status = request.Status
        });

        return Ok();
    }
}
    public record UpdateStatusRequest(int Id, string Status);
