using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dtos
{
    public readonly record struct UsuarioResponse
    (
        int IdUsuario,
        string Nombre,
        string Correo,
        string Contrasenia,
        string Telefono,
        List<CasaResponse>? Casas = null
    );
}