using BasErpFramework.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasErpFramework.Domain.Interface
{
    public interface IUserDomain
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
    }
}
