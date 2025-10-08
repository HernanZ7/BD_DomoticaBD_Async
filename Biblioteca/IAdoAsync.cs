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
        Task<IEnumerable<Electrodomestico>> ObtenerTodosLosElectrodomesticosAsync();
        Task<IEnumerable<Casa>> ObtenerTodasLasCasasAsync();
        Task<IEnumerable<Usuario>> ObtenerTodosLosUsuariosAsync();
        Task<bool> EliminarElectrodomesticoAsync(int id);

    }
}