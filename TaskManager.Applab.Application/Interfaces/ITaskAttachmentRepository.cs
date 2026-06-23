using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Interfaces;

public interface ITaskAttachmentRepository
{
    Task<TaskAttachment?> GetByIdAsync(int id);
    Task<List<TaskAttachment>> GetByTaskIdAsync(int taskId);
    Task AddAsync(TaskAttachment attachment);
    Task DeleteAsync(TaskAttachment attachment);
}