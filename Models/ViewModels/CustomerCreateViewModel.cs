// Models/ViewModels/CustomerCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class CustomerCreateViewModel
    {
        [Required] public string Name { get; set; } = string.Empty;

        [Required] public string DocumentNumber { get; set; } = string.Empty;

        [Required, EmailAddress] public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        // Opcional: si lo completan, se usa; si no, va un default seguro
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
