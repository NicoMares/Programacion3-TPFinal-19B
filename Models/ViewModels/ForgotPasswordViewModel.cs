// Models/ViewModels/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;
namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    }
}
