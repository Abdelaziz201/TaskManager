using Microsoft.EntityFrameworkCore;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Domain.Entities;
using TaskManager.Applab.Persistence;

namespace TaskManager.Applab.Persistence.Repositories;

public class TaskAttachmentRepository : ITaskAttachmentRepository
{
    private readonly AppDbContext _context;

    public TaskAttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskAttachment?> GetByIdAsync(int id)
    {
        return await _context.TaskAttachments.FindAsync(id);
    }

    public async Task<List<TaskAttachment>> GetByTaskIdAsync(int taskId)
    {
        return await _context.TaskAttachments
            .Where(a => a.TaskItemId == taskId)
            .ToListAsync();
    }

    // single task — used by GetTaskByIdAsync / CreateTaskAsync / UpdateTaskAsync
    public async Task<int> GetCountByTaskIdAsync(int taskId)
    {
        return await _context.TaskAttachments
            .CountAsync(a => a.TaskItemId == taskId);
    }

    // many tasks at once — used by GetAllTasksAsync, avoids N+1
    public async Task<Dictionary<int, int>> GetCountsByTaskIdsAsync(List<int> taskIds)
    {
        return await _context.TaskAttachments
            .Where(a => taskIds.Contains(a.TaskItemId))
            .GroupBy(a => a.TaskItemId)
            .Select(g => new { TaskItemId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TaskItemId, x => x.Count);
    }

    public async Task AddAsync(TaskAttachment attachment)
    {
        await _context.TaskAttachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskAttachment attachment)
    {
        _context.TaskAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }
}