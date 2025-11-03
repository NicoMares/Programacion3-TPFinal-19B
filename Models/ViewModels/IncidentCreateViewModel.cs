// Models/ViewModels/IncidentCreateViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentCreateViewModel
    {
        [Required] public Guid CustomerId { get; set; }
        [Required] public Guid TypeId { get; set; }
        [Required] public Guid PriorityId { get; set; }
        [Required] public Guid AssignedToUserId { get; set; }

        [Required, StringLength(2000)]
        public string Problem { get; set; } = null!;

        public IEnumerable<SelectListItem>? Customers { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
        public IEnumerable<SelectListItem>? Priorities { get; set; }
        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}
