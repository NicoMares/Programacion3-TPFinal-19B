using System;
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models
{
    public class PasswordResetToken
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid UserId { get; set; }
        [Required] public string Token { get; set; } = null!;
        [Required] public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}