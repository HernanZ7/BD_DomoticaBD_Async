namespace Biblioteca;

public class Casa
{
    public int IdCasa { get; set; }
    public required string Direccion { get; set; }
    public IEnumerable<Electrodomestico> Electros { get; set; } = [];
    public double ConsumoTotal { get; set; } = 0f;
    
}