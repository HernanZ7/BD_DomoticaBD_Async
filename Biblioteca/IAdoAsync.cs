using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca
{
    public interface IAdoAsync
    {
        Task AltaUsuarioAsync(Usuario usuario);
        Task AltaCasaAsync(Casa casa);
        Task AltaConsumoAsync(Consumo consumo);
        Task AltaHistorialRegistroAsync(HistorialRegistro historialRegistro);
        Task AltaElectrodomesticoAsync(Electrodomestico electrodomestico);
        Task<Electrodomestico?> ObtenerElectrodomesticoAsync(int IdElectrodomestico);
        Task<Casa?> ObtenerCasaAsync(int IdCasa);
        Task<Usuario?> UsuarioPorPassAsync(string Correo, string Contrasenia);
        Task UpdateUsuarioAsync(Usuario usuario);
        Task<IEnumerable<Electrodomestico>> ObtenerTodosLosElectrodomesticosAsync();
        Task<IEnumerable<Casa>> ObtenerTodasLasCasasAsync();
        Task<IEnumerable<Usuario>> ObtenerTodosLosUsuariosAsync();
        Task<IEnumerable<Consumo>> ObtenerConsumosPorCasaAsync(int idCasa);
        Task<bool> EliminarElectrodomesticoAsync(int id);
        Task<bool> EliminarCasaAsync(int id);
        Task<bool> EliminarUsuarioAsync(int id);
        Task<bool> AsignarCasaAUsuarioAsync(int idUsuario, int idCasa);
        Task<List<Casa>> ObtenerCasasPorUsuarioAsync(int idUsuario);
        Task<double> ObtenerConsumoTotalCasaAsync(int idCasa);
        Task<List<Electrodomestico>> ObtenerElectrosPorCasaAsync(int idCasa);
        Task<double> ObtenerConsumoTotalElectroAsync(int idElectrodomestico);
        Task<int> EliminarElectrosPorCasaAsync(int idCasa);
        Task<IEnumerable<Consumo>> ObtenerConsumosPorElectrodomesticoAsync(int idElectrodomestico);
        Task ActualizarEstadoElectrodomesticoAsync(int idElectrodomestico, bool encendido);
        Task ActualizarElectrodomesticoAsync(Electrodomestico e);
        Task ActualizarEstadoAsync(Electrodomestico e);
        Task InsertarInicioHistorialAsync(int idElectro, DateTime inicio);
        Task CrearRegistroConsumoAsync(int idElectro, DateTime inicio);
        Task FinalizarRegistroConsumoAsync(int idElectro, DateTime fin);
        Task<Consumo?> ObtenerConsumoActivoAsync(int idElectro);


    }
}