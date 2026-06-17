using Microsoft.AspNetCore.Http.HttpResults;
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

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Task with id {id} not found");//404
        }

        public async Task CreateTaskAsync(TaskItem task)
        {
            await _repository.AddAsync(task);
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            await _repository.UpdateAsync(task);
        }
        public async Task DeleteTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Task with id {id} not found");//404
            await _repository.DeleteAsync(task!);
            
            

        }
    }
}
