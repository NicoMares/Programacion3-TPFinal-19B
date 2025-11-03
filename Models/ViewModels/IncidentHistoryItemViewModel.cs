// Models/ViewModels/IncidentHistoryItemViewModel.cs
using System;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class IncidentHistoryItemViewModel
    {
        public DateTime When { get; set; }
        public string Kind { get; set; } = "";          // "Abierto" | "Reasignado" | "Resuelto" | "Cerrado"
        public string Who { get; set; } = "";           // username de quien ejecutó la acción
        public string? Detail { get; set; }             // ej: "a Telefonista 1" o nota de resolución/cierre
    }
}
