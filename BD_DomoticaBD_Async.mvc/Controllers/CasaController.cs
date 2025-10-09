using Microsoft.AspNetCore.Mvc;
using Biblioteca;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class CasaController : Controller
    {
        private readonly IAdoAsync _repo;

        public CasaController(IAdoAsync repo)
        {
            _repo = repo;
        }

        // ✅ Acción para listar todas las casas en una vista
        public async Task<IActionResult> GetAll()
        {
            var casas = await _repo.ObtenerTodasLasCasasAsync();
            return View(casas); // Vista: Views/Casa/GetAll.cshtml
        }

        // ✅ Vista de formulario de invitación
        [HttpGet]
        public IActionResult AltaForm() => View();

        [HttpPost]
        public async Task<IActionResult> AltaForm(Casa casa)
        {
            await _repo.AltaCasaAsync(casa);
            return RedirectToAction("GetAll"); // redirige al listado
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var casa = await _repo.ObtenerCasaAsync(id);

            if (casa == null)
            {
                return NotFound();
            }

            await _repo.EliminarCasaAsync(id);

            return RedirectToAction("GetAll");
        }

    }
}
