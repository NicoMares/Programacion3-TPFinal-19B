// Models/ViewModels/CustomerCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class CustomerCreateViewModel
    {
        public Guid Id { get; set; } // opcional en Create
        [System.ComponentModel.DataAnnotations.Required]

        [RegularExpression(@"^\d+$", ErrorMessage = "El número de documento NO puede Contener Caracteres.")]
        [MinLength(7, ErrorMessage = "El Numero De Documento debe tener al menos 7 Digitos.")]
        public string DocumentNumber { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required]
        [MinLength(3, ErrorMessage = "El Nombre debe tener al menos 3 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string Name { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
        [MinLength(16, ErrorMessage = "El email debe tener al menos 16 caracteres.")]
     
        public string Email { get; set; } = null!;

        [RegularExpression(@"^\d+$", ErrorMessage = "El número telefónico NO puede Contener Caracteres.")]
        [MinLength(10, ErrorMessage = "El Numero De Telefono debe tener al menos 10 Digitos.")]
        public string? Phone { get; set; }


    }
}
