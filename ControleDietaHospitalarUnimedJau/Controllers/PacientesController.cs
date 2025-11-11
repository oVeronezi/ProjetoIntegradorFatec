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

        public PacientesController(ContextMongodb context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // GET: Pacientes (LISTAGEM COM BUSCA + LOOKUP CORRIGIDO)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Index(string searchString)
        {
            // ----- INÍCIO DA LÓGICA DE BUSCA -----
            var filter = Builders<Paciente>.Filter.Empty;

            // Se a string de busca não for nula ou vazia, cria um filtro
            if (!String.IsNullOrEmpty(searchString))
            {
                // "i" torna a busca case-insensitive (ignora maiúsculas/minúsculas)
                // Isto filtra APENAS o campo "Nome" da coleção Pacientes.
                filter = Builders<Paciente>.Filter.Regex("Nome", new BsonRegularExpression(searchString, "i"));
            }

            ViewData["CurrentFilter"] = searchString; // Para manter o texto na caixa de busca
            // ----- FIM DA LÓGICA DE BUSCA -----


            // O pipeline de agregação com $lookup
            var pipeline = _context.Pacientes.Aggregate()
                // 1. Aplica o filtro de busca ($match)
                .Match(filter)

                // 2. Faz o $lookup
                // ----- CORREÇÃO DE NOME: "DietaVinculada" -> "DetalhesDieta" -----
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

        // ------------------------------------------------------------------
        // GET: Pacientes/Details/5 (LOOKUP CORRIGIDO)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id)
                // ----- CORREÇÃO DE NOME: "DietaVinculada" -> "DetalhesDieta" -----
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

        // ==================================================================
        // GET: Pacientes/Create (PREPARA FORMULÁRIO)
        // ==================================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta"
            );
            return View();
        }

        // ==================================================================
        // POST: Pacientes/Create (MODELSTATE CORRIGIDO)
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,NumQuarto,CodPulseira,IdDieta")] Paciente Paciente)
        {
            // ----- CORREÇÃO DE NOME: "DietaVinculada" -> "DetalhesDieta" -----
            ModelState.Remove("DetalhesDieta");

            // (A remoção de "Entregas" não é mais necessária pois removemos do modelo)

            if (ModelState.IsValid)
            {
                Paciente.Id = Guid.NewGuid();
                await _context.Pacientes.InsertOneAsync(Paciente);
                return RedirectToAction(nameof(Index));
            }

            // Recarrega o dropdown em caso de erro
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );
            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Edit/5 (CARREGA FORMULÁRIO E DIETAS)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var Paciente = await _context.Pacientes.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Paciente == null) return NotFound();

            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );

            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // POST: Pacientes/Edit/5 (MODELSTATE CORRIGIDO)
        // ------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome,NumQuarto,CodPulseira,IdDieta")] Paciente Paciente)
        {
            if (id != Paciente.Id) return NotFound();

            // ----- CORREÇÃO DE NOME: "DietaVinculada" -> "DetalhesDieta" -----
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

            // Recarrega o dropdown em caso de erro
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );
            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Delete/5 (LOOKUP CORRIGIDO)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id)
                // ----- CORREÇÃO DE NOME: "DietaVinculada" -> "DetalhesDieta" -----
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

        // ------------------------------------------------------------------
        // POST: Pacientes/Delete/5
        // ------------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Pacientes.DeleteOneAsync(u => u.Id == id);
            return RedirectToAction(nameof(Index));
        }

        // (O método PacientesExists foi removido pois não estava sendo usado)
    }
}