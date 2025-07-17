namespace Biblioteca.Persistencia.Dapper.Test;

public class HIstorialRegistroTest : TestBase
{
    public HIstorialRegistroTest() : base() { }
    [Fact]
    public void AltaHistorialRegistroOK()
    {
        int idElectrodomesticoValido = 1; 
        var Martes = new HistorialRegistro()
        {
            FechaHoraRegistro = DateTime.Now,
            IdElectrodomestico = idElectrodomesticoValido
        };

        Ado.AltaHistorialRegistro(Martes);
        Assert.NotEqual(0, Martes.IdElectrodomestico);
    }
}
