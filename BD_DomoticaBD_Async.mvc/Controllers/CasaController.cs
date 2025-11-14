using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using Biblioteca.Persistencia.Dapper;
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

        // ✅ Listar todas las casas del usuario logueado
        public async Task<IActionResult> GetAll()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var casas = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);

            // Recalcular consumo total desde electrodomésticos
            foreach (var casa in casas)
            {
                casa.ConsumoTotal = await _repo.ObtenerConsumoTotalCasaAsync(casa.IdCasa);
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(casas);
        }

        // ✅ Formulario de alta
        [HttpGet]
        public IActionResult AltaForm()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ✅ Alta de nueva casa
        [HttpPost]
        public async Task<IActionResult> AltaForm(Casa casa)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(casa);

            // Validar dirección única por usuario
            var casasUsuario = await _repo.ObtenerCasasPorUsuarioAsync(userId.Value);
            bool direccionDuplicada = casasUsuario.Any(c => c.Direccion == casa.Direccion);

            if (direccionDuplicada)
            {
                ViewBag.Error = "Ya existe una casa con esa dirección.";
                return View(casa);
            }

            await _repo.AltaCasaAsync(casa);
            await _repo.AsignarCasaAUsuarioAsync(userId.Value, casa.IdCasa);

            return RedirectToAction("GetAll");
        }

        // ✅ Eliminar una casa
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            await _repo.EliminarCasaAsync(id);
            return RedirectToAction("GetAll");
        }

        // ✅ Eliminar todas las casas del usuario
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
