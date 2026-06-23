using Microsoft.AspNetCore.Mvc;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Services;


namespace TaskManager.Applab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/task
    public class TaskController: ControllerBase
    {
        private readonly TaskServices _taskService;

        public TaskController(TaskServices taskServices)
        {
            _taskService = taskServices;
        }

        //Get /api/task
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        //Get /api/task/id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _taskService.GetTaskByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        //post /api/task
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var result = await _taskService.CreateTaskAsync(dto);
            return Ok(result);
        }

        //Put /api/task/id
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTaskDto dto)
        {
            var result = await _taskService.UpdateTaskAsync(id, dto);
            return result.Success ? Ok(result) : NotFound(result);
        }

        //Delete /api/task/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
