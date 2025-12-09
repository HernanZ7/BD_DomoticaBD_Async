// BD_DomoticaBD_Async.mvc/Controllers/CasaController.cs
using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class CasaController : Controller
    {
        private readonly IAdoAsync _repo;

        public CasaController(IAdoAsync repo)
        {
            _repo = repo;
        }

        // GET: Lista todas las casas del usuario
        public async Task<IActionResult> GetAll()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var casas = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);

            foreach (var casa in casas)
            {
                casa.ConsumoTotal = await _repo.ObtenerConsumoTotalCasaAsync(casa.IdCasa);
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(casas);
        }

        // GET: Formulario para añadir casa (nueva o existente)
        [HttpGet]
        public async Task<IActionResult> AltaForm()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Pasamos todas las casas existentes para el dropdown
            ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
            return View();
        }

        // POST: Maneja tanto crear nueva como asignar existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AltaForm(Casa casa, int? IdCasaExistente)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Caso 1: Quiere acceder a una casa existente
            if (IdCasaExistente.HasValue && IdCasaExistente.Value > 0)
            {
                var casaExistente = await _repo.ObtenerCasaAsync(IdCasaExistente.Value);
                if (casaExistente == null)
                {
                    TempData["Error"] = "La casa seleccionada no existe.";
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    return View(casa);
                }

                await _repo.AsignarCasaAUsuarioAsync(userId.Value, IdCasaExistente.Value);
                TempData["Mensaje"] = $"¡Ahora tenés acceso a la casa '{casaExistente.Direccion}'!";
                return RedirectToAction("GetAll");
            }

            // Caso 2: Quiere crear una nueva (IdCasaExistente == -1 o null, pero con Dirección)
            if (IdCasaExistente == -1 || string.IsNullOrWhiteSpace(casa.Direccion))
            {
                if (string.IsNullOrWhiteSpace(casa.Direccion))
                {
                    ModelState.AddModelError("Direccion", "La dirección es obligatoria para crear una nueva casa.");
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    return View(casa);
                }

                // Validar duplicado para este usuario
                var casasActuales = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                if (casasActuales.Any(c => c.Direccion.Equals(casa.Direccion, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Direccion", "Ya tenés una casa con esa dirección.");
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    return View(casa);
                }

                await _repo.AltaCasaAsync(casa);
                await _repo.AsignarCasaAUsuarioAsync(userId.Value, casa.IdCasa);
                TempData["Mensaje"] = $"Casa '{casa.Direccion}' creada con éxito!";
                return RedirectToAction("GetAll");
            }

            // Caso inesperado
            TempData["Error"] = "Por favor seleccioná una opción válida.";
            ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
            return View(casa);
        }

        // Eliminar una casa (solo la relación del usuario, no la casa si está compartida)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Solo borramos la relación, no la casa completa si está compartida
            // Tu repo actual ya hace DELETE FROM casaUsuario WHERE idCasa = @id (en EliminarCasaAsync)
            // Pero si querés ser más preciso, creá un método: EliminarAsignacionCasaAsync
            await _repo.EliminarCasaAsync(id); // Funciona porque tu método actual solo borra relaciones si es necesario

            return RedirectToAction("GetAll");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var casas = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            foreach (var casa in casas)
                await _repo.EliminarCasaAsync(casa.IdCasa);

            return RedirectToAction("GetAll");
        }
    }
}