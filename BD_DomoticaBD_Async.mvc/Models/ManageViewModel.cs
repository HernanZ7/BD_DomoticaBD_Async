// BD_DomoticaBD_Async.mvc/Models/ManageViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class ManageViewModel
    {
        public int IdUsuario { get; set; }

        [Required]
        public string Nombre { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = "";

        public string Telefono { get; set; } = "";
    }
}
