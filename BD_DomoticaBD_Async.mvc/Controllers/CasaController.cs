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
        // GET: Formulario
        [HttpGet]
        public async Task<IActionResult> AltaForm()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Cargar TODAS las casas (para compartir)
            ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();

            // Cargar las casas que YA tiene el usuario (para excluirlas del dropdown)
            ViewBag.CasasDelUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);

            return View();
        }

        // POST: Procesar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AltaForm(Casa casa, int? IdCasaExistente)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Caso: Crear nueva casa
            if (IdCasaExistente == -1)
            {
                if (string.IsNullOrWhiteSpace(casa.Direccion))
                {
                    ModelState.AddModelError("Direccion", "La dirección es obligatoria para crear una nueva casa.");
                }
                else
                {
                    var casasActuales = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                    if (casasActuales.Any(c => c.Direccion.Equals(casa.Direccion, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Direccion", "Ya tenés una casa con esa dirección.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    ViewBag.CasasDelUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                    return View(casa);
                }

                await _repo.AltaCasaAsync(casa);
                await _repo.AsignarCasaAUsuarioAsync(userId.Value, casa.IdCasa);
                TempData["Mensaje"] = $"Casa '{casa.Direccion}' creada con éxito!";
                return RedirectToAction("GetAll");
            }

            // Caso: Acceder a casa existente
            if (IdCasaExistente.HasValue && IdCasaExistente.Value > 0)
            {
                var casaExistente = await _repo.ObtenerCasaAsync(IdCasaExistente.Value);
                if (casaExistente == null)
                {
                    ModelState.AddModelError("IdCasaExistente", "La casa seleccionada no existe.");
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    ViewBag.CasasDelUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                    return View(casa);
                }

                // Verificar si ya la tiene (doble chequeo por seguridad)
                var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                if (casasUsuario.Any(c => c.IdCasa == IdCasaExistente.Value))
                {
                    ModelState.AddModelError("IdCasaExistente", $"Ya tenés acceso a la casa '{casaExistente.Direccion}'. Elegí otra o creá una nueva.");
                    ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
                    ViewBag.CasasDelUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
                    return View(casa);
                }

                await _repo.AsignarCasaAUsuarioAsync(userId.Value, IdCasaExistente.Value);
                TempData["Mensaje"] = $"¡Ahora tenés acceso a la casa '{casaExistente.Direccion}'!";
                return RedirectToAction("GetAll");
            }

            ModelState.AddModelError("", "Por favor seleccioná una opción válida.");
            ViewBag.TodasLasCasas = await _repo.ObtenerTodasLasCasasAsync();
            ViewBag.CasasDelUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            return View(casa);
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