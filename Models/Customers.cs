// Models/Customer.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models
{
    public class Customer
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required] public string DocumentNumber { get; set; } = null!;
        [Required] public string Name { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required] public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
