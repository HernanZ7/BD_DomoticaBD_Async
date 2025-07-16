using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca
{
    public interface IAdoAsync
    {
    Task AltaUsuario(Usuario usuario);
    Task AltaCasa (Casa casa);
    Task AltaConsumo (Consumo consumo);
    Task AltaHistorialRegistro (HistorialRegistro historialRegistro);
    Task AltaElectrodomestico (Electrodomestico electrodomestico);
    Task<Electrodomestico>? ObtenerElectrodomestico (int IdElectrodomestico);
    Task<Casa>? ObtenerCasa (int IdCasa);
    Task<Usuario>? UsuarioPorPass (string Correo, string Contrasenia);
    }
}