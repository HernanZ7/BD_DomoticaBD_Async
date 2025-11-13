namespace Biblioteca;
public class Electrodomestico
{
   public int IdElectrodomestico {get; set;}
   public int IdCasa {get; set;}
   public string Nombre { get; set; }
   public string Tipo { get; set; }
   public string Ubicacion { get; set; }
   public bool Encendido {get; set;}
   public bool Apagado{get; set;}
   public List<HistorialRegistro> ConsumoMensual { get; set; } = [];

}
