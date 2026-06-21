using Microsoft.EntityFrameworkCore;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Persistence.Repositories;

    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context; // injected automatically
        }

        //Task.find()
        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        //Task>findById(id)
        public async Task<TaskItem?> GetByIdAsync(int id) 
        {
            return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

    //New Task(data).Save
    public async Task AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        //findByIdAndUpdate
        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

    //findByIdAndDelete
    public async Task DeleteAsync(TaskItem task)
    {
        try
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Row was already deleted by a concurrent request — safe to ignore
        }
    }
}

