// Models/Incident.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models
{
    public class Incident
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(30)]
        public string Number { get; set; } = null!;

        [Required] public Guid CustomerId { get; set; }
        [Required] public Guid TypeId { get; set; }
        [Required] public Guid PriorityId { get; set; }

        [Required, StringLength(2000)]
        public string Problem { get; set; } = null!;

        [Required, StringLength(20)]
        public string State { get; set; } = "Abierto";

        [Required] public Guid OwnerUserId { get; set; }
        [Required] public Guid AssignedToUserId { get; set; }

        [StringLength(2000)] public string? ResolutionNote { get; set; }
        [StringLength(2000)] public string? CloseComment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required] public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
