using BasErpFramework.Domain.Entity;

namespace BasErpFramework.Infrastructure.Interface
{
    public interface IUsersRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user,string password);
        Task<bool> CheckPasswordAsync(User user, string password);
    }
}
