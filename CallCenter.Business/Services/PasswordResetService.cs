// CallCenter.Business/Services/PasswordResetService.cs
using System;
using CallCenter.Business.Security;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;

namespace CallCenter.Business.Services
{
    public class PasswordResetService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetTokenRepository _tokens;

        public PasswordResetService(IUserRepository users, IPasswordResetTokenRepository tokens)
        {
            _users = users;
            _tokens = tokens;
        }

        // Devuelve el token en texto (para armar el link); si el mail no existe, retorna string.Empty
        public string RequestReset(string email, TimeSpan ttl)
        {
            User u = _users.FindActiveByEmail(email);
            if (u == null) return string.Empty;

            _tokens.InvalidateAllForUser(u.Id); // opcional: invalidar anteriores

            string token = Crypto.GenerateUrlToken();
            string hash = Crypto.Sha256(token);
            DateTime expires = DateTime.UtcNow.Add(ttl);
            _tokens.Create(u.Id, hash, expires);
            return token;
        }

        public bool ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            string hash = Crypto.Sha256(token);
            PasswordResetToken prt = _tokens.GetValidByTokenHash(hash, DateTime.UtcNow);
            if (prt == null) return false;

            string newHash = Crypto.Sha256(newPassword);
            bool ok = _users.UpdatePassword(prt.UserId, newHash);
            if (!ok) return false;

            _tokens.MarkUsed(prt.Id);
            return true;
        }
    }
}
