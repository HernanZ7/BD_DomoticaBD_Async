namespace Biblioteca.Persistencia.Dapper.TestAsync;

public class ConsumoTest : TestBaseAsync
{
    public ConsumoTest() : base() { }

    [Fact]
    public async Task AltaConsumoOK ()
    {
        
        var Lunes = new Consumo ()
        {
            Inicio = DateTime.Now,
            Duracion = new TimeSpan(2, 0, 0), 
            ConsumoTotal = 9.3F,
            IdElectrodomestico = 1,
        };

        await AdoAsync.AltaConsumoAsync(Lunes);

        Assert.NotEqual(0, Lunes.IdConsumo);
    }
}