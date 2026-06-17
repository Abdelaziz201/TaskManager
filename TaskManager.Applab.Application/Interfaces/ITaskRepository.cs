using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}
