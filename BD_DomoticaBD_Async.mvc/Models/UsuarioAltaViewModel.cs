using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class UsuarioAltaViewModel
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contrasenia { get; set; }
        public string Telefono { get; set; }

        public List<int> CasasSeleccionadas { get; set; } = new List<int>();
        public List<Biblioteca.Casa> TodasLasCasas { get; set; } = new List<Biblioteca.Casa>();
    }
}
