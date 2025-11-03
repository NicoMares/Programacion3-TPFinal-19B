// Models/ViewModels/IncidentCommentItemViewModel.cs
using System;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentCommentItemViewModel
    {
        public Guid Id { get; set; }
        public Guid AuthorUserId { get; set; }
        public string AuthorUsername { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
