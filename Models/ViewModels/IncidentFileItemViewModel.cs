using System;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentFileItemViewModel
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = "";
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string Path { get; set; } = ""; // relativo para mostrar link directo
    }
}
