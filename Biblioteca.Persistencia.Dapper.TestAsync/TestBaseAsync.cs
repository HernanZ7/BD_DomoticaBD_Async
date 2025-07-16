using MySqlConnector;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Biblioteca.Persistencia.Dapper.TestAsync;
public class TestBaseAsync
{
    private readonly IDbConnection Conexion;
    protected readonly IAdoAsync AdoAsync;

    public TestBaseAsync()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
            .Build();
        string cadena = config.GetConnectionString("MySQL")!;
        Conexion = new MySqlConnection(cadena);

        AdoAsync = new AdoDapperAsync(Conexion);
    }
}
