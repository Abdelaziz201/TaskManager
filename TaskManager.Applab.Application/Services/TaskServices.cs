using TaskManager.Applab.Application.Common;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Services
{
    public class TaskServices
    {
        private readonly ITaskRepository _repository;

        public TaskServices(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<TaskItem>>> GetAllTasksAsync()
        {
            var tasks = await _repository.GetAllAsync();
            return ApiResponse<List<TaskItem>>.Ok(tasks, "Tasks retrieved successfully");
        }

        public async Task<ApiResponse<TaskItem>> GetTaskByIdAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return ApiResponse<TaskItem>.Fail($"Task with id {id} not found");

            return ApiResponse<TaskItem>.Ok(task);
        }

        public async Task<ApiResponse<TaskItem>> CreateTaskAsync(TaskItem task)
        {
            await _repository.AddAsync(task);
            return ApiResponse<TaskItem>.Ok(task, "Task created successfully");
        }

        public async Task<ApiResponse<TaskItem>> UpdateTaskAsync(TaskItem task)
        {
            await _repository.UpdateAsync(task);
            return ApiResponse<TaskItem>.Ok(task, "Task updated successfully");
        }

        public async Task<ApiResponse> DeleteTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return ApiResponse.Fail($"Task with id {id} not found");

            await _repository.DeleteAsync(task);
            return ApiResponse.Ok("Task deleted successfully");
        }
    }
}