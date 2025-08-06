namespace Biblioteca.Persistencia.Dapper.TestAsync;

public class UserTest : TestBaseAsync
{
    public UserTest() : base() { }

    [Fact]
    public async Task AltaUsuarioOK()
    {
        var brenda = new Usuario()
        {
            Nombre = "Brenda",
            Telefono = "238238",
            Correo = "bren@da2.gmail",
            Contrasenia = "123456"
        };

        await AdoAsync.AltaUsuarioAsync(brenda);

        Assert.NotNull(brenda);
        Assert.NotEqual(0, brenda.IdUsuario);
    }

    [Fact]
    public async Task UsuarioPorPassOKAsync()
    {
        var usuario = await AdoAsync.UsuarioPorPassAsync("bren@da2.gmail", "123456");
        
        Assert.NotNull(usuario);
        Assert.Equal(1, usuario.IdUsuario);
    }
}