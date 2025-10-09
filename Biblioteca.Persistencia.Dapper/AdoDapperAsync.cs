using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace Biblioteca.Persistencia.Dapper
{
    public class AdoDapperAsync : IAdoAsync
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
            WHERE Correo = @Correo and Contrasenia = SHA2(@Contrasenia, 256);";

        public AdoDapperAsync(IDbConnection conexion)
        => _conexion = conexion;
        public async Task AltaCasaAsync(Casa casa)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@unidCasa", direction: ParameterDirection.Output);
            parametros.Add("@unDireccion", casa.Direccion);

            await _conexion.ExecuteAsync("altaCasa", parametros); // Carga el sp y los parametros desde dapper.

            casa.IdCasa = parametros.Get<int>("@unidCasa");
        }

        public async Task AltaConsumoAsync(Consumo consumo)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@unidConsumo", direction: ParameterDirection.Output);
            parametros.Add("@unidElectrodomestico", consumo.IdElectrodomestico);
            parametros.Add("@uninicio", consumo.Inicio);
            parametros.Add("@unDuracion", consumo.Duracion);
            parametros.Add("@unConsumoTotal", consumo.ConsumoTotal);

            await _conexion.ExecuteAsync("altaConsumo", parametros);

            consumo.IdConsumo = parametros.Get<int>("@unidConsumo");
        }

        public async Task AltaElectrodomesticoAsync(Electrodomestico electrodomestico)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@unidElectrodomestico", direction: ParameterDirection.Output);
            parametros.Add("@unidCasa", electrodomestico.IdCasa);
            parametros.Add("@unNombre", electrodomestico.Nombre);
            parametros.Add("@unTipo", electrodomestico.Tipo);
            parametros.Add("@unUbicacion", electrodomestico.Ubicacion);
            parametros.Add("@unEncendido", electrodomestico.Encendido);
            parametros.Add("@unApagado", electrodomestico.Apagado);

            await _conexion.ExecuteAsync("altaElectrodomestico", parametros);

            electrodomestico.IdElectrodomestico = parametros.Get<int>("@unidElectrodomestico");
        }

        public async Task AltaHistorialRegistroAsync(HistorialRegistro historialRegistro)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@unidElectrodomestico", historialRegistro.IdElectrodomestico);
            parametros.Add("@unFechaHoraRegistro", historialRegistro.FechaHoraRegistro);

            await _conexion.ExecuteAsync("altaHistorialRegistro", parametros, commandType: CommandType.StoredProcedure);
        }

        public async Task AltaUsuarioAsync(Usuario usuario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@unidUsuario", direction: ParameterDirection.Output);
            parametros.Add("@unNombre", usuario.Nombre);
            parametros.Add("@unCorreo", usuario.Correo);
            parametros.Add("@uncontrasenia", usuario.Contrasenia);
            parametros.Add("@unTelefono", usuario.Telefono);

            await _conexion.ExecuteAsync("altaUsuario", parametros);

            usuario.IdUsuario = parametros.Get<int>("@unidUsuario");
        }

        public async Task<Casa>? ObtenerCasaAsync(int idCasa)
        {
            using (var registro = await _conexion.QueryMultipleAsync(_queryCasa, new { id = idCasa }))
            {
                var casa = await registro.ReadSingleOrDefaultAsync<Casa>();
                if (casa is not null)
                {
                    casa.Electros = await registro.ReadAsync<Electrodomestico>();
                }
                return casa;
            }
        }

        public async Task<Electrodomestico>? ObtenerElectrodomesticoAsync(int idElectrodomestico)
        {
            using (var registro = await _conexion.QueryMultipleAsync(_queryElectrodomestico, new { id = idElectrodomestico }))
            {
                var electrodomestico = await registro.ReadSingleOrDefaultAsync<Electrodomestico>();
                if (electrodomestico is not null)
                {
                    var PasarALista = await registro.ReadAsync<HistorialRegistro>();
                    electrodomestico.ConsumoMensual = PasarALista.ToList();
                }
                return electrodomestico;
            }
        }

        public async Task<Usuario>? UsuarioPorPassAsync(string Correo, string Contrasenia)
        {
            var usuario = await _conexion.QueryFirstOrDefaultAsync<Usuario>(_queryUsuario, new { Correo, Contrasenia });

            return usuario;
        }
        public async Task<IEnumerable<Electrodomestico>> ObtenerTodosLosElectrodomesticosAsync()
        {
            var sql = "SELECT * FROM Electrodomestico";
            return await _conexion.QueryAsync<Electrodomestico>(sql);
        }

        public async Task<IEnumerable<Casa>> ObtenerTodasLasCasasAsync()
        {
            var sql = "SELECT * FROM Casa";
            return await _conexion.QueryAsync<Casa>(sql);
        }
        public async Task<IEnumerable<Usuario>> ObtenerTodosLosUsuariosAsync()
        {
            var sql = "SELECT * FROM Usuario";
            var usuarios = await _conexion.QueryAsync<Usuario>(sql);

            foreach (var usuario in usuarios)
            {
                var sqlCasas = @"
                    SELECT c.*
                    FROM Casa c
                    INNER JOIN casaUsuario cu ON c.idCasa = cu.idCasa
                    WHERE cu.idUsuario = @IdUsuario";

                var casas = await _conexion.QueryAsync<Casa>(sqlCasas, new { usuario.IdUsuario });
                usuario.ListadoCasas = casas.ToList();
            }

            return usuarios;
        }

        public async Task<IEnumerable<Consumo>> ObtenerConsumosPorCasaAsync(int idCasa)
        {
            var sql = @"SELECT c.*
                        FROM Consumo c
                        INNER JOIN Electrodomestico e ON c.idElectrodomestico = e.idElectrodomestico
                        WHERE e.idCasa = @IdCasa";
            return await _conexion.QueryAsync<Consumo>(sql, new { IdCasa = idCasa });
        }

        public async Task<bool> EliminarElectrodomesticoAsync(int id)
        {
            var sqlHistorialRegistro = "DELETE FROM HistorialRegistro WHERE idElectrodomestico = @IdElectrodomestico";
            await _conexion.ExecuteAsync(sqlHistorialRegistro, new { IdElectrodomestico = id });

            var sqlConsumo = "DELETE FROM Consumo WHERE idElectrodomestico = @IdElectrodomestico";
            await _conexion.ExecuteAsync(sqlConsumo, new { IdElectrodomestico = id });

            var sqlElectrodomestico = "DELETE FROM Electrodomestico WHERE idElectrodomestico = @IdElectrodomestico";
            var result = await _conexion.ExecuteAsync(sqlElectrodomestico, new { IdElectrodomestico = id });

            return result > 0;
        }

        public async Task<bool> EliminarCasaAsync(int id)
        {
            var sqlHistorialRegistro = "DELETE FROM HistorialRegistro WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa)";
            await _conexion.ExecuteAsync(sqlHistorialRegistro, new { IdCasa = id });

            var sqlConsumo = "DELETE FROM Consumo WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa)";
            await _conexion.ExecuteAsync(sqlConsumo, new { IdCasa = id });

            var sqlElectrodomestico = "DELETE FROM Electrodomestico WHERE idCasa = @IdCasa";
            await _conexion.ExecuteAsync(sqlElectrodomestico, new { IdCasa = id });

            var sqlCasa = "DELETE FROM Casa WHERE idCasa = @IdCasa";
            var result = await _conexion.ExecuteAsync(sqlCasa, new { IdCasa = id });

            return result > 0;
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var sqlUsuario = "DELETE FROM Usuario WHERE idUsuario = @IdUsuario";
            var result = await _conexion.ExecuteAsync(sqlUsuario, new { IdUsuario = id });

            return result > 0;
        }
        
        public async Task AsignarCasaAUsuarioAsync(int idUsuario, int idCasa)
        {
            var query = "INSERT INTO casaUsuario (IdUsuario, IdCasa) VALUES (@idUsuario, @idCasa)";
            await _conexion.ExecuteAsync(query, new { idUsuario, idCasa });
        }
    }
}