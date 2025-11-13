using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class Casa
    {
        public int IdCasa { get; set; }
        public string Direccion { get; set; } = "";
        public double ConsumoTotal { get; set; }
    }
}
