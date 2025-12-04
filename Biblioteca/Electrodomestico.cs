namespace Biblioteca
{
   public class Electrodomestico
   {
      public int IdElectrodomestico { get; set; }
      public int IdCasa { get; set; }
      public string Nombre { get; set; }
      public string Tipo { get; set; }
      public string Ubicacion { get; set; }
      public bool Encendido { get; set; }
      public bool Apagado { get; set; }

      // PROPIEDADES NECESARIAS PARA EL CONTROLADOR
      public DateTime? Inicio { get; set; }     // Cuándo se encendió
      public float ConsumoPorHora { get; set; }
      public float ConsumoTotal { get; set; }   // Suma de consumos

      // Mantengo tu lista
      public List<HistorialRegistro> ConsumoMensual { get; set; } = new();
   }
}
