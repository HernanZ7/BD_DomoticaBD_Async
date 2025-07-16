namespace Biblioteca.Persistencia.Dapper.TestAsync;

public class CasaTest : TestBaseAsync
{
    public CasaTest() : base() { }

    [Fact]
    public async Task AltaCasaOK()
    {
        var CasaRetiro = new Casa()
        {
            Direccion = "Colibri 111"
        };

        await AdoAsync.AltaCasa(CasaRetiro);

        Assert.NotEqual(0, CasaRetiro.IdCasa);
    }

    [Fact]
    public async Task ObtenerCasaOK()
    {
        var Casa = await AdoAsync.ObtenerCasa(2);
        
        Assert.NotNull(Casa);
        Assert.Equal(2, Casa.IdCasa);
        Assert.Equal("Libertador 284", Casa.Direccion);
    }
}
