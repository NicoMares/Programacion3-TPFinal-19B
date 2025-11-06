// CallCenter.Domain/Entities/PasswordResetToken.cs
using System;

namespace CallCenter.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
