// CallCenter.Domain/Abstractions/IUserRepository.cs
using System;
using CallCenter.Domain.Entities;

namespace CallCenter.Domain.Abstractions
{
    public interface IUserRepository
    {
        User FindActiveByEmail(string email);
        bool ExistsByUsernameOrEmail(string username, string email);
        Guid InsertTelefonista(User user);
        bool UpdatePassword(Guid userId, string newPasswordHash);
        User GetById(Guid id);
    }
}
