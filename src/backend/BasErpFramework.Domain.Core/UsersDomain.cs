using BasErpFramework.Domain.Entity;
using BasErpFramework.Domain.Interface;
using BasErpFramework.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasErpFramework.Domain.Core
{

    public class UsersDomain : IUserDomain
    {
        private readonly IUnitOfWork _unitOfWork;
        public UsersDomain(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _unitOfWork.Users.CheckPasswordAsync(user, password);
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            return await _unitOfWork.Users.CreateUserAsync(user, password);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }
    }
}
