// BD_DomoticaBD_Async.mvc/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using BD_DomoticaBD_Async.mvc.Models;
using MinimalApi.Dtos;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAdoAsync _repo;
        public AccountController(IAdoAsync repo) => _repo = repo;

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya tiene sesión, redirigir a lista de casas
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("GetAll", "Casa");

            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _repo.UsuarioPorPassAsync(model.Correo!, model.Contrasenia!);

            if (usuario is null)
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(model);
            }

            // Guardar en session
            HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
            HttpContext.Session.SetString("UserName", usuario.Nombre);
            HttpContext.Session.SetString("UserEmail", usuario.Correo);

            return RedirectToAction("GetAll", "Casa");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(UsuarioAltaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var nuevo = new Biblioteca.Usuario
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Contrasenia = model.Contrasenia,
                Telefono = model.Telefono
            };

            try
            {
                await _repo.AltaUsuarioAsync(nuevo);
            }
            catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(nameof(model.Correo), "El correo ya se encuentra registrado.");
                return View(model);
            }

            // Iniciar sesión automáticamente tras registro
            HttpContext.Session.SetInt32("UserId", nuevo.IdUsuario);
            HttpContext.Session.SetString("UserName", nuevo.Nombre);
            HttpContext.Session.SetString("UserEmail", nuevo.Correo);

            return RedirectToAction("GetAll", "Casa");
        }

        // POST: /Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Manage
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction(nameof(Login));

            var lista = await _repo.ObtenerTodosLosUsuariosAsync();
            var usuario = lista.FirstOrDefault(u => u.IdUsuario == userId.Value);
            if (usuario == null) return RedirectToAction(nameof(Logout));

            var model = new ManageViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono
            };

            return View(model);
        }

        // POST: /Account/Manage
        [HttpPost]
        public async Task<IActionResult> Manage(ManageViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = new Biblioteca.Usuario
            {
                IdUsuario = model.IdUsuario,
                Nombre = model.Nombre,
                Correo = model.Correo,
                Telefono = model.Telefono,
                Contrasenia = "" // necesario porque la clase Usuario marca 'required'
            };

            try
            {
                await _repo.UpdateUsuarioAsync(usuario);
                HttpContext.Session.SetString("UserName", usuario.Nombre);
                HttpContext.Session.SetString("UserEmail", usuario.Correo);
                ViewBag.Message = "Datos actualizados correctamente.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Duplicate") || ex.Message.Contains("1062"))
                    ModelState.AddModelError(nameof(model.Correo), "El correo ya está en uso.");
                else
                    ModelState.AddModelError("", "Error al actualizar el usuario.");
            }

            return View(model);
        }



        // POST: /Account/DeleteAccount
        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction(nameof(Login));

            // Obtener todas las casas del usuario
            var usuarios = await _repo.ObtenerTodosLosUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.IdUsuario == userId.Value);
            if (usuario == null) return RedirectToAction(nameof(Logout));

            // Borrar todas las casas del usuario (cada Eliminacion borra electro y historial)
            foreach (var casa in usuario.ListadoCasas)
            {
                await _repo.EliminarCasaAsync(casa.IdCasa);
            }

            // Borrar usuario
            await _repo.EliminarUsuarioAsync(userId.Value);

            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
