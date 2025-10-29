﻿// Models/ViewModels/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;
namespace Progra3_TPFinal_19B.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required] public string Token { get; set; } = string.Empty;
        [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
    }
}
