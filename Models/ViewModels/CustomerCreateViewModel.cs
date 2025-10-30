// Models/ViewModels/CustomerCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class CustomerCreateViewModel
    {
        public Guid Id { get; set; } // opcional en Create
        [System.ComponentModel.DataAnnotations.Required]
        public string DocumentNumber { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = null!;

        public string? Phone { get; set; }


    }
}
