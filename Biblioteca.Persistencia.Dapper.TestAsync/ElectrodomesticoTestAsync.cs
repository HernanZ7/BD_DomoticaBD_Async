namespace Biblioteca.Persistencia.Dapper.TestAsync;

public class ElectrodomesticoTest : TestBaseAsync
{
    public ElectrodomesticoTest() : base() { }

    [Fact]
    public async Task AltaElectrodomesticoOK ()
    {
        var Lavarropa = new Electrodomestico()
        {
            IdCasa = 1,
            Nombre = "AGH123",
            Tipo = "Lavaropa",
            Ubicacion = "Lavanderia",
            Encendido = false,
        };
        await AdoAsync.AltaElectrodomesticoAsync(Lavarropa);

        Assert.NotEqual(0, Lavarropa.IdElectrodomestico);
        
    }

    [Fact]
    public async Task ObtenerElectrodomesticoOK()
    {
        var lavarropa = await AdoAsync.ObtenerElectrodomesticoAsync(1);  
        
        Assert.NotNull(lavarropa);
        Assert.Equal("Lavarropa", lavarropa.Nombre);
        Assert.Equal(1, lavarropa.IdElectrodomestico);
        Assert.NotEmpty(lavarropa.ConsumoMensual);
    }
}
