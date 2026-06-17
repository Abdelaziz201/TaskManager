using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Services;
using TaskManager.Applab.Domain.Entities;

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
            var task = await _taskService.GetTaskByIdAsync(id);
            return Ok(task);
        }

        //post /api/task
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
            };
            await _taskService.CreateTaskAsync(task);
            return Ok(task);
        }

        //Put /api/task/id
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTaskDto dto)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _taskService.UpdateTaskAsync(task);
            return Ok(task);
        }

        //Delete /api/task/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            return Ok(new { message= "Task deleted"});
        }
    }
}
