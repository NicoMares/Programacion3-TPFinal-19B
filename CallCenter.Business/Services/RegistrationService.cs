using System;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;
using CallCenter.Business.Security;

namespace CallCenter.Business.Services
{
    public class RegistrationService
    {
        private readonly IUserRepository _users;

        public RegistrationService(IUserRepository users)
        {
            _users = users;
        }

        public bool RegisterTelefonista(string username, string fullName, string email, string plainPassword, out Guid newUserId, out string error)
        {
            newUserId = Guid.Empty;
            error = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(plainPassword))
            {
                error = "Datos incompletos.";
                return false;
            }

            if (_users.ExistsByUsernameOrEmail(username, email))
            {
                error = "Ya existe un usuario con ese nombre de usuario o email.";
                return false;
            }

            User u = new User();
            u.Username = username.Trim();
            u.FullName = fullName.Trim();
            u.Email = email.Trim();
            u.PasswordHash = PasswordHasher.Sha256(plainPassword); 

            newUserId = _users.InsertTelefonista(u);
            return newUserId != Guid.Empty;
        }
    }
}
