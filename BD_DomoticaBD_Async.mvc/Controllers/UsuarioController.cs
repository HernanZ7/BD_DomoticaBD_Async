using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Biblioteca;
using MinimalApi.Dtos;
using BD_DomoticaBD_Async.mvc.Models;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IAdoAsync _repo;

        public UsuarioController(IAdoAsync repo)
        {
            _repo = repo;
        }

        [HttpGet("{correo}, {contrasenia}")]
        public async Task<IActionResult> Get(string correo, string contrasenia)
        {
            var usuario = await _repo.UsuarioPorPassAsync(correo, contrasenia);
            if (usuario is null) return NotFound();

            var response = new UsuarioResponse(
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Correo,
                usuario.Contrasenia,
                usuario.Telefono
            );

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _repo.ObtenerTodosLosUsuariosAsync();

            var response = lista.Select(u => new UsuarioResponse(
                u.IdUsuario,
                u.Nombre,
                u.Correo,
                u.Contrasenia,
                u.Telefono,
                u.ListadoCasas.Select(c => new CasaResponse(c.IdCasa, c.Direccion)).ToList()
            )).ToList();

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> AltaForm()
        {
            var casas = await _repo.ObtenerTodasLasCasasAsync(); // Trae todas las casas
            var model = new UsuarioAltaViewModel
            {
                TodasLasCasas = casas.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AltaForm(UsuarioAltaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TodasLasCasas = (await _repo.ObtenerTodasLasCasasAsync()).ToList();
                return View(model);
            }

            var usuario = new Biblioteca.Usuario
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Contrasenia = model.Contrasenia,
                Telefono = model.Telefono
            };
            await _repo.AltaUsuarioAsync(usuario);

            foreach (var idCasa in model.CasasSeleccionadas)
            {
                await _repo.AsignarCasaAUsuarioAsync(usuario.IdUsuario, idCasa);
            }

            return RedirectToAction("GetAll");
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearUsuarioRequest request)
        {
            var nuevoUsuario = new Biblioteca.Usuario
            {
                IdUsuario = request.IdUsuario,
                Nombre = request.Nombre,
                Correo = request.Correo,
                Contrasenia = request.Contrasenia,
                Telefono = request.Telefono
            };

            await _repo.AltaUsuarioAsync(nuevoUsuario);

            var response = new UsuarioResponse(
                nuevoUsuario.IdUsuario,
                nuevoUsuario.Nombre,
                nuevoUsuario.Correo,
                nuevoUsuario.Contrasenia,
                nuevoUsuario.Telefono
            );

            return CreatedAtAction(nameof(Get), new { correo = nuevoUsuario.Correo }, response);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _repo.ObtenerTodosLosUsuariosAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            await _repo.EliminarUsuarioAsync(id);

            return RedirectToAction("GetAll");
        }

    }
}