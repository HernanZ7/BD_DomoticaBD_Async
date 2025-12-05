using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Biblioteca.Persistencia.Dapper;
public class AdoDapper : IAdo
{
    readonly IDbConnection _conexion;
    private readonly string _queryElectrodomestico
    = @"SELECT  *
    FROM    Electrodomestico
    WHERE   idElectrodomestico = @id;

    SELECT  *
    FROM    HistorialRegistro
    WHERE   idElectrodomestico = @id;";

    private readonly string _queryCasa
    = @"SELECT *
        FROM Casa
        WHERE idCasa = @id;
        
        SELECT  *
        FROM    Electrodomestico
        WHERE   idCasa = @id;";

    private readonly string _queryUsuario
    = @"SELECT *
        FROM Usuario
        WHERE Correo = @Correo 
        AND Contrasenia = SHA2(@Contrasenia, 256);";

    public AdoDapper(IDbConnection conexion)
    => _conexion = conexion;

    public async void AltaUsuario(Usuario usuario)
    {
        var parametros = new DynamicParameters();
        parametros.Add("@unidUsuario", direction: ParameterDirection.Output);
        parametros.Add("@unNombre", usuario.Nombre);
        parametros.Add("@unCorreo", usuario.Correo);
        parametros.Add("@unContrasenia", usuario.Contrasenia);
        parametros.Add("@unTelefono", usuario.Telefono);

        _conexion.Execute("altaUsuario", parametros);

        usuario.IdUsuario = parametros.Get<int>("@unidUsuario");
    }

    public void AltaCasa(Casa casa)
    {
        var parametros = new DynamicParameters();
        parametros.Add("@unidCasa", direction: ParameterDirection.Output);
        parametros.Add("@unDireccion", casa.Direccion);

        _conexion.Execute("altaCasa", parametros); // Carga el sp y los parametros desde dapper.

        casa.IdCasa = parametros.Get<int>("@unidCasa");
    }

    public void AltaElectrodomestico(Electrodomestico electrodomestico)
    {
        var parametros = new DynamicParameters();
        parametros.Add("@unidElectrodomestico", direction: ParameterDirection.Output);
        parametros.Add("@unidCasa", electrodomestico.IdCasa);
        parametros.Add("@unNombre", electrodomestico.Nombre);
        parametros.Add("@unTipo", electrodomestico.Tipo);
        parametros.Add("@unUbicacion", electrodomestico.Ubicacion);
        parametros.Add("@unEncendido", electrodomestico.Encendido);

        _conexion.Execute("altaElectrodomestico", parametros);

        electrodomestico.IdElectrodomestico = parametros.Get<int>("@unidElectrodomestico");
    }

    public void AltaHistorialRegistro(HistorialRegistro historialRegistro)
    {
        var parametros = new DynamicParameters();
        parametros.Add("@unidElectrodomestico", historialRegistro.IdElectrodomestico);
        parametros.Add("@unFechaHoraRegistro", historialRegistro.FechaHoraRegistro);

        _conexion.Execute("altaHistorialRegistro", parametros, commandType: CommandType.StoredProcedure);

    }

    public void AltaConsumo(Consumo consumo)
    {
        var parametros = new DynamicParameters();
        parametros.Add("@unidConsumo", direction: ParameterDirection.Output);
        parametros.Add("@unidElectrodomestico", consumo.IdElectrodomestico);
        parametros.Add("@uninicio", consumo.Inicio);
        parametros.Add("@unDuracion", consumo.Duracion);
        parametros.Add("@unConsumoTotal", consumo.ConsumoTotal);

        _conexion.Execute("altaConsumo", parametros);

        consumo.IdConsumo = parametros.Get<int>("@unidConsumo");
    }
    // query de Electrodomestico
    public Electrodomestico? ObtenerElectrodomestico(int idElectrodomestico)
    {
        using (var registro = _conexion.QueryMultiple(_queryElectrodomestico, new { id = idElectrodomestico }))
        {
            var electrodomestico = registro.ReadSingleOrDefault<Electrodomestico>();
            if (electrodomestico is not null)
            {
                electrodomestico.ConsumoMensual = registro.Read<HistorialRegistro>().ToList();
            }
            return electrodomestico;
        }
    }
    // query de Casa
    public Casa? ObtenerCasa(int idCasa)
    {
        using (var registro = _conexion.QueryMultiple(_queryCasa, new { id = idCasa }))
        {
            var casa = registro.ReadSingleOrDefault<Casa>();
            if (casa is not null)
            {
                casa.Electros = registro.Read<Electrodomestico>();
            }
            return casa;
        }
    }
    // query de Usuario
    public Usuario? UsuarioPorPass(string Correo, string Contrasenia)
    {
        var usuario = _conexion.QueryFirstOrDefault<Usuario>(_queryUsuario, new {Correo, Contrasenia });

        
        return usuario;
    }
}

