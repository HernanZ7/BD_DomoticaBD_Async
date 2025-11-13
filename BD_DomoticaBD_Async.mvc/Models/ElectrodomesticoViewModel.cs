using System;
using System.Collections.Generic;

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

        // Datos calculados para UI
        public double ConsumoTotal { get; set; } = 0.0;

        // Historial simple (opcional)
        public List<Biblioteca.Consumo> Consumos { get; set; } = new List<Biblioteca.Consumo>();
    }
}
