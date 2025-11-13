using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using BD_DomoticaBD_Async.mvc.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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
                vm.Add(new ElectrodomesticoViewModel
                {
                    IdElectrodomestico = e.IdElectrodomestico,
                    IdCasa = e.IdCasa,
                    Nombre = e.Nombre,
                    Tipo = e.Tipo,
                    Ubicacion = e.Ubicacion,
                    Encendido = e.Encendido,
                    Apagado = e.Apagado,
                    ConsumoTotal = consumo
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

            // NUEVO: validar Ubicación única por casa para mostrar mensaje amigable
            if (!string.IsNullOrWhiteSpace(electro.Ubicacion))
            {
                var existe = await _repo.UbicacionExisteEnCasaAsync(electro.IdCasa, electro.Ubicacion);
                if (existe)
                {
                    ModelState.AddModelError(nameof(electro.Ubicacion), "La ubicación ya está en uso en esta casa.");
                    ViewBag.IdCasa = electro.IdCasa;
                    return View(electro);
                }
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
                ConsumoTotal = await _repo.ObtenerConsumoTotalElectroAsync(electro.IdElectrodomestico)
            };

            var consumos = await _repo.ObtenerConsumosPorElectrodomesticoAsync(electro.IdElectrodomestico);
            vm.Consumos = consumos?.ToList() ?? new List<Biblioteca.Consumo>();

            ViewBag.IdCasa = electro.IdCasa;

            return View(vm);
        }

        public async Task<IActionResult> Edit(int idElectrodomestico)
        {
            var e = await _repo.ObtenerElectrodomesticoAsync(idElectrodomestico);
            return View(e);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Biblioteca.Electrodomestico e)
        {
            if (!ModelState.IsValid)
                return View(e);

            await _repo.ActualizarElectrodomesticoAsync(e);
            return RedirectToAction("Index", new { idCasa = e.IdCasa });
        }
        
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idElectrodomestico, bool encendido)
        {
            // validación de sesión (opcional pero recomendable)
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Autorizar que el electro pertenezca al usuario (si implementaste ObtenerCasasPorUsuarioAsync)
            var electro = await _repo.ObtenerElectrodomesticoAsync(idElectrodomestico);
            if (electro == null) return NotFound();

            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            if (!casasUsuario.Any(c => c.IdCasa == electro.IdCasa))
                return Forbid();

            // Actualizar estado en BD (debes tener este método en IAdoAsync / AdoDapperAsync)
            await _repo.ActualizarEstadoElectrodomesticoAsync(idElectrodomestico, encendido);

            // Si querés: al apagar, registrar consumo / historial -> lo agregamos luego.
            return Ok();
        }


    }
}
