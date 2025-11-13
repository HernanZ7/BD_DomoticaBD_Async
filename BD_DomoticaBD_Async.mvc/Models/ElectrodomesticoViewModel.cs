using System;
using System.Collections.Generic;
using Biblioteca;

namespace BD_DomoticaBD_Async.mvc.Models
{
    public class ElectrodomesticoViewModel
    {
        public int IdElectrodomestico { get; set; }
        public int IdCasa { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public bool Encendido { get; set; }
        public bool Apagado { get; set; }

        // Consumptions
        // ConsumoTotal acumulado (kWh)
        public double ConsumoTotal { get; set; }

        // Lista histórica de consumos (desde la tabla Consumo)
        public List<Consumo> Consumos { get; set; } = new();

        // Momento de inicio actual (si está encendido). Nullable por si no hay sesión activa.
        public DateTime? Inicio { get; set; }
    }
}
