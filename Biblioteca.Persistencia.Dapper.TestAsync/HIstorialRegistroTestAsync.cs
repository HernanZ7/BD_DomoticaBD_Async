namespace Biblioteca.Persistencia.Dapper.TestAsync;

public class HIstorialRegistroTestAsync : TestBaseAsync
{
    public HIstorialRegistroTestAsync() : base() { }
    [Fact]
    public async Task AltaHistorialRegistroOK()
    {
        int idElectrodomesticoValido = 1; 
        var Martes = new HistorialRegistro()
        {
            FechaHoraRegistro = DateTime.Now,
            IdElectrodomestico = idElectrodomesticoValido
        };

        await AdoAsync.AltaHistorialRegistro(Martes);
        Assert.NotEqual(0, Martes.IdElectrodomestico);
    }
}
