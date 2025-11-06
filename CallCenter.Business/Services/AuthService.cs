using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;
using CallCenter.Business.Security;
using System;

namespace CallCenter.Business.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        public AuthService(IUserRepository users)
        {
            _users = users;
        }

        public bool TryLogin(string email, string password, out User user)
        {
            user = _users.FindActiveByEmail(email);
            if (user == null) return false;

            var hash = PasswordHasher.Sha256(password);
            return string.Equals(hash, user.PasswordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
