// BD_DomoticaBD_Async.mvc/Models/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string? Correo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Contrasenia { get; set; }
    }
}
