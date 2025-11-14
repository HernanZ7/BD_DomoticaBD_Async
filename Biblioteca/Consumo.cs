namespace Biblioteca
{
    public class Consumo
    {
        public int IdConsumo { get; set; }
        public int IdElectrodomestico { get; set; }
        public DateTime Inicio { get; set; }
        public TimeSpan Duracion { get; set; }
        public float ConsumoTotal { get; set; }
    }
}
