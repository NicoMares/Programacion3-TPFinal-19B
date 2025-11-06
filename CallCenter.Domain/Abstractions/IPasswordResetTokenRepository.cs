// CallCenter.Domain/Abstractions/IPasswordResetTokenRepository.cs
using System;
using CallCenter.Domain.Entities;

namespace CallCenter.Domain.Abstractions
{
    public interface IPasswordResetTokenRepository
    {
        Guid Create(Guid userId, string tokenHash, DateTime expiresAt);
        PasswordResetToken GetValidByTokenHash(string tokenHash, DateTime nowUtc);
        bool MarkUsed(Guid id);
        bool InvalidateAllForUser(Guid userId); // opcional: invalidar anteriores
    }
}
