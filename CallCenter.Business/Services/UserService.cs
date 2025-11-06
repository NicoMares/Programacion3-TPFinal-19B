using System;
using System.Security.Cryptography;
using System.Text;
using CallCenter.Domain.Abstractions;

namespace CallCenter.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        public UserService(IUserRepository users) { _users = users; }

        public bool ValidateCredentials(string email, string plainPassword, out string role)
        {
            role = null;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(plainPassword))
                return false;

            var user = _users.FindActiveByEmail(email);
            if (user == null) return false;

            var hash = Sha256(plainPassword);
            if (!string.Equals(user.PasswordHash, hash, StringComparison.OrdinalIgnoreCase))
                return false;

            role = user.Role;
            return true;
        }

        private static string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
