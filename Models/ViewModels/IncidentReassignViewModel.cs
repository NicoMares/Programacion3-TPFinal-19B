// Models/ViewModels/IncidentReassignViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentReassignViewModel
    {
        [Required] public Guid Id { get; set; }
        public string Number { get; set; } = "";

        [Required] public Guid AssignedToUserId { get; set; }
        public string? Reason { get; set; }

        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}
