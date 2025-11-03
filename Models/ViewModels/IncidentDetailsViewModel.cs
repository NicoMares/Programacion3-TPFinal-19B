// Models/ViewModels/IncidentDetailsViewModel.cs  (agregá la lista de comentarios)
using System;
using System.Collections.Generic;


namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string PriorityName { get; set; } = "";
        public string State { get; set; } = "";
        public Guid AssignedToUserId { get; set; }
        public string AssignedToUsername { get; set; } = "";
        public string Problem { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public IEnumerable<IncidentCommentItemViewModel> Comments { get; set; } = Array.Empty<IncidentCommentItemViewModel>();
        public IEnumerable<IncidentFileItemViewModel> Files { get; set; } = Array.Empty<IncidentFileItemViewModel>();
        

    }
}
