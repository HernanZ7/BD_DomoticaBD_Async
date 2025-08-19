using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dtos
{
    public readonly record struct CrearElectrodomesticoRequest
    (
        int IdCasa,
        string Nombre,
        string Tipo,
        string Ubicacion,
        bool Encendido,
        bool Apagado
    );
}