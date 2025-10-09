using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class UsuarioDetalleViewModel
    {
        public Usuario usuario { get; set; }
        public List<Casa> casas { get; set; }
    }
}