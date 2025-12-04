using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using BD_DomoticaBD_Async.mvc.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class ElectrodomesticoController : Controller
    {
        private readonly IAdoAsync _repo;

        public ElectrodomesticoController(IAdoAsync repo)
        {
            _repo = repo;
        }

        // GET: /Electrodomestico/GetAll?idCasa=XX
        public async Task<IActionResult> GetAll(int idCasa)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == idCasa))
                return Forbid();

            var electros = await _repo.ObtenerElectrosPorCasaAsync(idCasa);

            var vm = new List<ElectrodomesticoViewModel>();
            foreach (var e in electros)
            {
                var consumo = await _repo.ObtenerConsumoTotalElectroAsync(e.IdElectrodomestico);

                // Obtener consumo activo únicamente si hay uno (filtrado en el repo)
                var consumoActivo = await _repo.ObtenerConsumoActivoAsync(e.IdElectrodomestico);

                vm.Add(new ElectrodomesticoViewModel
                {
                    IdElectrodomestico = e.IdElectrodomestico,
                    IdCasa = e.IdCasa,
                    Nombre = e.Nombre,
                    Tipo = e.Tipo,
                    Ubicacion = e.Ubicacion,
                    Encendido = e.Encendido,
                    Apagado = e.Apagado,
                    ConsumoTotal = consumo,
                    Inicio = consumoActivo?.Inicio
                });
            }

            ViewBag.IdCasa = idCasa;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(vm);
        }

        // GET: /Electrodomestico/AltaForm?idCasa=XX
        [HttpGet]
        public async Task<IActionResult> AltaForm(int idCasa)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == idCasa))
                return Forbid();

            var model = new Biblioteca.Electrodomestico { IdCasa = idCasa };
            ViewBag.IdCasa = idCasa;
            return View(model);
        }

        // POST: /Electrodomestico/AltaForm
        [HttpPost]
        public async Task<IActionResult> AltaForm(Biblioteca.Electrodomestico electro)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == electro.IdCasa))
                return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.IdCasa = electro.IdCasa;
                return View(electro);
            }
            await _repo.AltaElectrodomesticoAsync(electro);

            return RedirectToAction("GetAll", new { idCasa = electro.IdCasa });
        }

        // POST: /Electrodomestico/Delete?id=XX
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var electro = await _repo.ObtenerElectrodomesticoAsync(id);
            if (electro == null) return NotFound();

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == electro.IdCasa))
                return Forbid();

            await _repo.EliminarElectrodomesticoAsync(id);
            return RedirectToAction("GetAll", new { idCasa = electro.IdCasa });
        }

        // POST: /Electrodomestico/DeleteAll?idCasa=XX
        [HttpPost]
        public async Task<IActionResult> DeleteAll(int idCasa)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == idCasa))
                return Forbid();

            await _repo.EliminarElectrosPorCasaAsync(idCasa);

            return RedirectToAction("GetAll", new { idCasa });
        }

        // GET: /Electrodomestico/Detalle?id=XX
        public async Task<IActionResult> Detalle(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var electro = await _repo.ObtenerElectrodomesticoAsync(id);
            if (electro == null) return NotFound();

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == electro.IdCasa))
                return Forbid();

            var vm = new ElectrodomesticoViewModel
            {
                IdElectrodomestico = electro.IdElectrodomestico,
                IdCasa = electro.IdCasa,
                Nombre = electro.Nombre,
                Tipo = electro.Tipo,
                Ubicacion = electro.Ubicacion,
                Encendido = electro.Encendido,
                Apagado = electro.Apagado,
                ConsumoTotal = await _repo.ObtenerConsumoTotalElectroAsync(electro.IdElectrodomestico),
                ConsumoPorHora = electro.ConsumoPorHora
            };

            var consumos = await _repo.ObtenerConsumosPorElectrodomesticoAsync(electro.IdElectrodomestico);
            vm.Consumos = consumos?.ToList() ?? new List<Biblioteca.Consumo>();

            // Si existe un consumo ACTIVO (duracion='00:00:00' / consumoTotal = 0), lo consideramos inicio actual
            var consumoActivo = await _repo.ObtenerConsumoActivoAsync(electro.IdElectrodomestico);
            vm.Inicio = consumoActivo?.Inicio;

            ViewBag.IdCasa = electro.IdCasa;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEncendido([FromBody] ToggleEncendidoDto data)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var electro = await _repo.ObtenerElectrodomesticoAsync(data.idElectrodomestico);
            if (electro == null) return NotFound();

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == electro.IdCasa))
                return Forbid();

            if (data.encendido && !electro.Encendido)
            {
                // ENCENDER: actualizar estado y crear registro en Consumo
                await _repo.ActualizarEstadoElectrodomesticoAsync(electro.IdElectrodomestico, true);
                await _repo.CrearRegistroConsumoAsync(electro.IdElectrodomestico, DateTime.Now);

                // (Opcional) registrar timestamp en HistorialRegistro si quieres mantener log de eventos
                // await _repo.InsertarInicioHistorialAsync(electro.IdElectrodomestico, DateTime.Now);
            }
            else if (!data.encendido && electro.Encendido)
            {
                // APAGAR: actualizar estado y finalizar registro en Consumo
                await _repo.ActualizarEstadoElectrodomesticoAsync(electro.IdElectrodomestico, false);
                await _repo.FinalizarRegistroConsumoAsync(electro.IdElectrodomestico, DateTime.Now);

                // (Opcional) insertar marca de apagado en HistorialRegistro
                // await _repo.InsertarInicioHistorialAsync(electro.IdElectrodomestico, DateTime.Now);
            }

            return Ok();
        }

        // (Opcional) Toggle que redirige (mantengo para compatibilidad)
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var electro = await _repo.ObtenerElectrodomesticoAsync(id);

            if (electro == null)
                return NotFound();

            if (!electro.Encendido)
            {
                // ENCENDER
                electro.Encendido = true;
                electro.Apagado = false;

                await _repo.ActualizarElectrodomesticoAsync(electro);
                await _repo.CrearRegistroConsumoAsync(id, DateTime.Now);
            }
            else
            {
                // APAGAR
                electro.Encendido = false;
                electro.Apagado = true;

                await _repo.ActualizarElectrodomesticoAsync(electro);
                await _repo.FinalizarRegistroConsumoAsync(id, DateTime.Now);
            }

            return RedirectToAction("GetAll", new { idCasa = electro.IdCasa });
        }
    }
}
