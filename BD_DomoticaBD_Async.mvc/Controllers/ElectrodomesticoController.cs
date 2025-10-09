using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Biblioteca;
using MinimalApi.Dtos;

namespace BD_DomoticaBD_Async.mvc.Controllers
{
    public class ElectrodomesticoController : Controller
    {
        private readonly IAdoAsync _repo;

        public ElectrodomesticoController(IAdoAsync repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var electro = await _repo.ObtenerElectrodomesticoAsync(id);
            if (electro is null) return NotFound();

            var response = new ElectrodomesticoResponse(
                electro.IdElectrodomestico,
                electro.IdCasa,
                electro.Nombre,
                electro.Tipo,
                electro.Ubicacion,
                electro.Encendido,
                electro.Apagado
            );

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _repo.ObtenerTodosLosElectrodomesticosAsync();
            var response = lista.Select(e => new ElectrodomesticoResponse(
                e.IdElectrodomestico,
                e.IdCasa,
                e.Nombre,
                e.Tipo,
                e.Ubicacion,
                e.Encendido,
                e.Apagado
            )).ToList();

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> AltaForm()
        {
            var casas = await _repo.ObtenerTodasLasCasasAsync();
            ViewBag.Casas = casas; // Pasar las casas a la vista
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AltaForm(Electrodomestico electrodomestico)
        {
            await _repo.AltaElectrodomesticoAsync(electrodomestico);
            return RedirectToAction("GetAll"); // Redirige al listado
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearElectrodomesticoRequest request)
        {
            var nuevoElectro = new Electrodomestico
            {
                IdCasa = request.IdCasa,
                Nombre = request.Nombre,
                Tipo = request.Tipo,
                Ubicacion = request.Ubicacion,
                Encendido = request.Encendido,
                Apagado = request.Apagado
            };

            await _repo.AltaElectrodomesticoAsync(nuevoElectro);

            var response = new ElectrodomesticoResponse(
                nuevoElectro.IdElectrodomestico,
                nuevoElectro.IdCasa,
                nuevoElectro.Nombre,
                nuevoElectro.Tipo,
                nuevoElectro.Ubicacion,
                nuevoElectro.Encendido,
                nuevoElectro.Apagado
            );

            return CreatedAtAction(nameof(Get), new { id = nuevoElectro.IdElectrodomestico }, response);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var electrodomestico = await _repo.ObtenerElectrodomesticoAsync(id);

            if (electrodomestico == null)
            {
                return NotFound();
            }

            await _repo.EliminarElectrodomesticoAsync(id); 

            return RedirectToAction("GetAll");
        }
    }
}