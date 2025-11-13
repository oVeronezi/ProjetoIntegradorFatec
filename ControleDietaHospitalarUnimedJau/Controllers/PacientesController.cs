using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson; // Necessário para o Regex

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class PacientesController : Controller
    {
        private readonly ContextMongodb _context;
        // Filtro padrão para buscar apenas pacientes ativos
        private readonly FilterDefinition<Paciente> _filtroAtivos;

        public PacientesController(ContextMongodb context)
        {
            _context = context;
            // Define o filtro padrão
            _filtroAtivos = Builders<Paciente>.Filter.Eq(p => p.Ativo, true);
        }

        // GET: Pacientes (Correto)
        public async Task<IActionResult> Index(string searchString)
        {
            var filter = _filtroAtivos;

            if (!String.IsNullOrEmpty(searchString))
            {
                var filterBusca = Builders<Paciente>.Filter.Regex("Nome", new BsonRegularExpression(searchString, "i"));
                filter = Builders<Paciente>.Filter.And(_filtroAtivos, filterBusca);
            }

            ViewData["CurrentFilter"] = searchString;

            var pipeline = _context.Pacientes.Aggregate()
                .Match(filter)
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DetalhesDieta
                )
                .Unwind<Paciente, Paciente>(
                    p => p.DetalhesDieta,
                    new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                )
                .ToListAsync();

            return View(await pipeline);
        }

        // GET: Pacientes/Details/5 (Correto)
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id & p.Ativo == true) // Filtro de ativo
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DetalhesDieta
                )
                .Unwind<Paciente, Paciente>(
                    p => p.DetalhesDieta,
                    new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                );

            var Paciente = await pipeline.FirstOrDefaultAsync();

            if (Paciente == null) return NotFound();

            return View(Paciente);
        }

        // GET: Pacientes/Create (Corrigido o Dropdown)
        public async Task<IActionResult> Create()
        {
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(d => d.Ativo == true).ToListAsync(), // <-- FILTRO APLICADO
                "Id",
                "NomeDieta"
            );
            return View();
        }

        // POST: Pacientes/Create (Corrigido o Set de 'Ativo')
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,NumQuarto,CodPulseira,IdDieta")] Paciente Paciente)
        {
            ModelState.Remove("DetalhesDieta");

            if (ModelState.IsValid)
            {
                Paciente.Id = Guid.NewGuid();
                Paciente.Ativo = true;

                await _context.Pacientes.InsertOneAsync(Paciente);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(d => d.Ativo == true).ToListAsync(), // <-- FILTRO APLICADO
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );
            return View(Paciente);
        }

        // GET: Pacientes/Edit/5 (Corrigido o Dropdown)
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var Paciente = await _context.Pacientes.Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();
            if (Paciente == null) return NotFound();

            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(d => d.Ativo == true).ToListAsync(), // <-- FILTRO APLICADO
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );

            return View(Paciente);
        }

        // POST: Pacientes/Edit/5 (Correto)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome,NumQuarto,CodPulseira,IdDieta,Ativo")] Paciente Paciente) // Adicionado 'Ativo' ao Bind
        {
            if (id != Paciente.Id) return NotFound();

            ModelState.Remove("DetalhesDieta");

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _context.Pacientes.ReplaceOneAsync(m => m.Id == Paciente.Id, Paciente);
                    if (result.MatchedCount == 0) return NotFound();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Ocorreu um erro ao tentar salvar o paciente.");
                }
            }

            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(d => d.Ativo == true).ToListAsync(), // <-- FILTRO APLICADO
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );
            return View(Paciente);
        }

        // GET: Pacientes/Delete/5 (Correto)
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            // Filtro de ativo (correto)
            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id & p.Ativo == true)
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DetalhesDieta
                )
                .Unwind<Paciente, Paciente>(
                    p => p.DetalhesDieta,
                    new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                );

            var Paciente = await pipeline.FirstOrDefaultAsync();
            if (Paciente == null) return NotFound();

            return View(Paciente);
        }

        // POST: Pacientes/Delete/5 (Correto - Soft Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var filter = Builders<Paciente>.Filter.Eq(p => p.Id, id);
            var update = Builders<Paciente>.Update.Set(p => p.Ativo, false); // Define Ativo = false
            await _context.Pacientes.UpdateOneAsync(filter, update);
            return RedirectToAction(nameof(Index));
        }
    }
}