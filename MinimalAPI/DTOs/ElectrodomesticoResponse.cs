using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dtos
{
    public readonly record struct ElectrodomesticoResponse
    (
        int IdElectrodomestico,
        int IdCasa,
        string Nombre,
        string Tipo,
        string Ubicacion,
        bool Encendido
    );
}