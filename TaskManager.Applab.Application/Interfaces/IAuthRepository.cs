using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(User user);

        Task<User?> GetByIdAsync(int id);
        Task UpdateUserAsync(User user);
    }
}
