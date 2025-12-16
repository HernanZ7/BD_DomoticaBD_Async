using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class Electrodomestico
    {
        public int IdElectrodomestico { get; set; }
        public int IdCasa { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public bool Encendido { get; set; }
    }
}
