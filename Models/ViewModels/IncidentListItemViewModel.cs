// Models/ViewModels/IncidentListItemViewModel.cs
using System;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentListItemViewModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string PriorityName { get; set; } = "";
        public string State { get; set; } = "";
        public string AssignedToUsername { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
