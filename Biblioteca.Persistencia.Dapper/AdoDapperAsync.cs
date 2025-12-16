using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Linq;
using System.Collections.Generic;
using System;

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
            parametros.Add("@unConsumoPorHora", electrodomestico.ConsumoPorHora);

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
            // 1) Borrar relaciones casaUsuario
            var sqlCasaUsuario = "DELETE FROM casaUsuario WHERE idCasa = @IdCasa";
            await _conexion.ExecuteAsync(sqlCasaUsuario, new { IdCasa = id });

            // 2) Borrar historial
            var sqlHistorialRegistro = @"DELETE FROM HistorialRegistro 
                                        WHERE idElectrodomestico IN 
                                            (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa)";
            await _conexion.ExecuteAsync(sqlHistorialRegistro, new { IdCasa = id });

            // 3) Borrar consumos
            var sqlConsumo = @"DELETE FROM Consumo 
                            WHERE idElectrodomestico IN 
                                    (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa)";
            await _conexion.ExecuteAsync(sqlConsumo, new { IdCasa = id });

            // 4) Borrar electrodomésticos
            var sqlElectrodomestico = "DELETE FROM Electrodomestico WHERE idCasa = @IdCasa";
            await _conexion.ExecuteAsync(sqlElectrodomestico, new { IdCasa = id });

            // 5) Borrar la casa
            var sqlCasa = "DELETE FROM Casa WHERE idCasa = @IdCasa";
            var result = await _conexion.ExecuteAsync(sqlCasa, new { IdCasa = id });

            return result > 0;
        }


        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            try
            {
                // 1) Obtener las casas asociadas al usuario (antes de borrar relaciones)
                var casas = (await _conexion.QueryAsync<int>(
                    "SELECT idCasa FROM casaUsuario WHERE idUsuario = @IdUsuario",
                    new { IdUsuario = id })).ToList();

                // 2) Borrar relaciones del usuario en la tabla intermedia
                await _conexion.ExecuteAsync(
                    "DELETE FROM casaUsuario WHERE idUsuario = @IdUsuario",
                    new { IdUsuario = id });

                // 3) Para cada casa obtenida, si ya no tiene relaciones con otros usuarios,
                //    borrar dependencias en el orden correcto y luego la casa.
                foreach (var idCasa in casas)
                {
                    var restantes = await _conexion.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(1) FROM casaUsuario WHERE idCasa = @IdCasa",
                        new { IdCasa = idCasa });

                    if (restantes > 0)
                        continue; // casa compartida, no borrar

                    // Borrar dependencias (orden importante según FKs)
                    await _conexion.ExecuteAsync(
                        "DELETE FROM HistorialRegistro WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa);",
                        new { IdCasa = idCasa });

                    await _conexion.ExecuteAsync(
                        "DELETE FROM Consumo WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @IdCasa);",
                        new { IdCasa = idCasa });

                    await _conexion.ExecuteAsync(
                        "DELETE FROM Electrodomestico WHERE idCasa = @IdCasa;",
                        new { IdCasa = idCasa });

                    await _conexion.ExecuteAsync(
                        "DELETE FROM Casa WHERE idCasa = @IdCasa;",
                        new { IdCasa = idCasa });
                }

                // 4) Borrar el usuario
                var rows = await _conexion.ExecuteAsync(
                    "DELETE FROM Usuario WHERE idUsuario = @IdUsuario;",
                    new { IdUsuario = id });

                return rows > 0;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> AsignarCasaAUsuarioAsync(int idUsuario, int idCasa)
        {
            const string sql = "INSERT IGNORE INTO casaUsuario (idUsuario, idCasa) VALUES (@idUsuario, @idCasa)";
            var filasInsertadas = await _conexion.ExecuteAsync(sql, new { idUsuario, idCasa });

            return filasInsertadas > 0; // true = se asignó nueva, false = ya existía
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            var sql = @"UPDATE Usuario
                        SET Nombre = @Nombre,
                            Correo = @Correo,
                            Telefono = @Telefono
                        WHERE idUsuario = @IdUsuario;";
            // Esto lanzará excepción si Correo ya existe (clave única) — capturarás esto en el controlador.
            await _conexion.ExecuteAsync(sql, new { Nombre = usuario.Nombre, Correo = usuario.Correo, Telefono = usuario.Telefono, IdUsuario = usuario.IdUsuario });
        }

        public async Task<List<Casa>> ObtenerCasasPorUsuarioAsync(int idUsuario)
        {
            var sql = @"
                SELECT 
                    c.idCasa AS IdCasa,
                    c.Direccion,
                    COALESCE(SUM(co.consumoTotal), 0) AS ConsumoTotal
                FROM Casa c
                INNER JOIN casaUsuario cu ON c.idCasa = cu.idCasa
                LEFT JOIN Electrodomestico e ON e.idCasa = c.idCasa
                LEFT JOIN Consumo co ON co.idElectrodomestico = e.idElectrodomestico
                WHERE cu.idUsuario = @idUsuario
                GROUP BY c.idCasa, c.Direccion;
            ";

            var casas = (await _conexion.QueryAsync<Casa>(sql, new { idUsuario })).ToList();

            foreach (var casa in casas)
            {
                if (double.IsNaN(casa.ConsumoTotal))
                    casa.ConsumoTotal = 0.0;
            }

            return casas;
        }

        public async Task<double> ObtenerConsumoTotalCasaAsync(int idCasa)
        {
            string sql = @"
                SELECT COALESCE(SUM(c.ConsumoTotal), 0)
                FROM Consumo c
                INNER JOIN Electrodomestico e ON c.idElectrodomestico = e.idElectrodomestico
                WHERE e.idCasa = @idCasa;
            ";

            return await _conexion.ExecuteScalarAsync<double>(sql, new { idCasa });
        }

        // Obtiene todos los electrodomésticos que pertenecen a una casa
        public async Task<List<Electrodomestico>> ObtenerElectrosPorCasaAsync(int idCasa)
        {
            var sql = @"SELECT * FROM Electrodomestico WHERE idCasa = @idCasa";
            var list = (await _conexion.QueryAsync<Electrodomestico>(sql, new { idCasa })).ToList();
            return list;
        }

        // Suma el consumo (tabla Consumo) para un electrodoméstico dado
        public async Task<double> ObtenerConsumoTotalElectroAsync(int idElectrodomestico)
        {
            // consumoTotal = consumoPorHora * (duracion en horas)

            var sql = @"
                SELECT 
                    COALESCE(SUM(e.ConsumoPorHora * TIME_TO_SEC(c.duracion) / 3600), 0)
                FROM Consumo c
                INNER JOIN Electrodomestico e using(idElectrodomestico)
                WHERE idElectrodomestico = @idElectro;
            ";

            var result = await _conexion.ExecuteScalarAsync<double>(
                sql,
                new { idElectro = idElectrodomestico }
            );

            return result;
        }


        // Elimina todos los electrodomésticos (y sus consumos/historiales) de una casa
        public async Task<int> EliminarElectrosPorCasaAsync(int idCasa)
        {
            // Borrar HistorialRegistro para todos los electros de la casa
            var sqlHist = @"DELETE FROM HistorialRegistro 
                            WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @idCasa)";
            await _conexion.ExecuteAsync(sqlHist, new { idCasa });

            // Borrar consumos
            var sqlCons = @"DELETE FROM Consumo 
                            WHERE idElectrodomestico IN (SELECT idElectrodomestico FROM Electrodomestico WHERE idCasa = @idCasa)";
            await _conexion.ExecuteAsync(sqlCons, new { idCasa });

            // Borrar electrodomésticos y devolver cantidad afectada
            var sqlElectro = @"DELETE FROM Electrodomestico WHERE idCasa = @idCasa";
            var deleted = await _conexion.ExecuteAsync(sqlElectro, new { idCasa });

            return deleted; // cantidad de filas afectadas en Electrodomestico
        }

        // Obtener consumos (tabla Consumo) por idElectrodomestico
        public async Task<IEnumerable<Consumo>> ObtenerConsumosPorElectrodomesticoAsync(int idElectrodomestico)
        {
            var sql = @"SELECT * FROM Consumo WHERE idElectrodomestico = @idElectro ORDER BY inicio DESC";
            return await _conexion.QueryAsync<Consumo>(sql, new { idElectro = idElectrodomestico });
        }

        public async Task ActualizarEstadoElectrodomesticoAsync(int idElectrodomestico, bool encendido)
        {
            // Si encendido == true -> Encendido = 1, Apagado = 0
            // Si encendido == false -> Encendido = 0, Apagado = 1
            string query = @"UPDATE Electrodomestico 
                            SET Encendido = @encendido
                            WHERE idElectrodomestico = @idElectrodomestico";

            await _conexion.ExecuteAsync(query, new { idElectrodomestico, encendido });
        }

        public async Task ActualizarEstadoAsync(Electrodomestico e)
        {
            string sql = @"UPDATE Electrodomestico
                        SET Encendido = @Encendido,
                            Inicio = @Inicio,
                            ConsumoTotal = @ConsumoTotal
                        WHERE IdElectrodomestico = @IdElectrodomestico";

            await _conexion.ExecuteAsync(sql, e);
        }

        public async Task InsertarInicioHistorialAsync(int idElectro, DateTime inicio)
        {
            string sql = @"INSERT INTO HistorialRegistro (idElectrodomestico, fechaHoraRegistro)
                        VALUES (@idElectro, @inicio)";

            await _conexion.ExecuteAsync(sql, new { idElectro, inicio });
        }

        public async Task CrearRegistroConsumoAsync(int idElectro, DateTime inicio)
        {
            string sql = @"
                INSERT INTO Consumo (idElectrodomestico, inicio, duracion, consumoTotal)
                VALUES (@idElectro, @inicio, '00:00:00', 0)";
            await _conexion.ExecuteAsync(sql, new { idElectro, inicio });
        }

        public async Task<Consumo?> ObtenerConsumoActivoAsync(int idElectro)
        {
            string sql = @"
                SELECT * 
                FROM Consumo 
                WHERE idElectrodomestico = @idElectro
                AND (duracion = '00:00:00' OR consumoTotal = 0)
                ORDER BY idConsumo DESC
                LIMIT 1";

            return await _conexion.QueryFirstOrDefaultAsync<Consumo>(sql, new { idElectro });
        }

        public async Task FinalizarRegistroConsumoAsync(int idElectro, DateTime fin)
        {
            var dbConn = _conexion as IDbConnection;
            if (dbConn == null) throw new InvalidOperationException("La conexión no es un DbConnection.");

            try
            {
                if (dbConn.State != ConnectionState.Open)
                    dbConn.Open();

                var sqlGet = @"
                SELECT idConsumo
                FROM Consumo
                WHERE idElectrodomestico = @idElectro
                ORDER BY idConsumo DESC
                LIMIT 1;
            ";
                var idConsumo = await dbConn.QueryFirstOrDefaultAsync<int?>(sqlGet, new { idElectro });

                if (!idConsumo.HasValue)
                    return;

                var sqlUpd = @"
                UPDATE Consumo c
                JOIN Electrodomestico e ON e.idElectrodomestico = c.idElectrodomestico
                SET 
                    c.duracion = TIMEDIFF(@fin, c.inicio),
                    c.consumoTotal = (TIME_TO_SEC(TIMEDIFF(@fin, c.inicio)) / 3600) * e.ConsumoPorHora
                WHERE c.idConsumo = @idConsumo;
            ";
                await dbConn.ExecuteAsync(sqlUpd, new { fin, idConsumo = idConsumo.Value });
            }
            finally
            {
                if (dbConn.State != ConnectionState.Closed)
                    dbConn.Close();
            }
        }



        public async Task ActualizarElectrodomesticoAsync(Electrodomestico electro)
        {
            string sql = @"
                UPDATE Electrodomestico SET
                    Nombre = @Nombre,
                    Tipo = @Tipo,
                    Ubicacion = @Ubicacion,
                    Encendido = @Encendido,
                    Apagado = @Apagado
                WHERE idElectrodomestico = @IdElectrodomestico;
            ";

            await _conexion.ExecuteAsync(sql, electro);
        }

        public async Task CasaAUsuarioAsync(int idUsuario, int idCasa)
        {
            var parametros = new DynamicParameters();  // Crea params.
            parametros.Add("@unIdUsuario", idUsuario);  // Param para SP.
            parametros.Add("@unIdCasa", idCasa);  // Param para SP.
            await _conexion.ExecuteAsync("asignarCasaAUsuario", parametros, commandType: CommandType.StoredProcedure);  // Ejecuta SP async.
        }
    }
}