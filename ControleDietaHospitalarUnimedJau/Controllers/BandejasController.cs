using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    [Authorize]
    public class BandejasController : Controller
    {
        private readonly ContextMongodb _context;
        // --- CORREÇÃO 1: Adiciona o filtro padrão de "Ativos" ---
        private readonly FilterDefinition<Bandeja> _filtroAtivos;

        public BandejasController(ContextMongodb context)
        {
            _context = context;
            _filtroAtivos = Builders<Bandeja>.Filter.Eq(b => b.Ativo, true);
        }
        private async Task CarregarDietasViewBag(object? selectedDieta = null)
        {
            // --- CORREÇÃO 2: Filtra apenas Dietas Ativas no Dropdown ---
            var dietas = await _context.Dietas.Find(d => d.Ativo == true)
                                           .SortBy(d => d.NomeDieta)
                                           .ToListAsync();

            ViewBag.TipoDieta = new SelectList(dietas, "Id", "NomeDieta", selectedDieta);
        }


        // GET: Bandejas
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var filter = _filtroAtivos;

            if (!String.IsNullOrEmpty(searchString))
            {
                var filterBusca = Builders<Bandeja>.Filter.Regex("CodBandeja", new BsonRegularExpression(searchString, "i"));
                filter = Builders<Bandeja>.Filter.And(_filtroAtivos, filterBusca);
            }

            ViewData["CurrentFilter"] = searchString;

            var pipeline = _context.Bandejas.Aggregate()
                .Match(filter)
                .SortBy(b => b.CodBandeja) // <-- CORREÇÃO: ORDENAÇÃO AQUI
                .Lookup("Dietas", "TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Bandeja> { PreserveNullAndEmptyArrays = true });

            return View(await pipeline.ToListAsync());
        }

        // GET: Bandejas/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 4: Combina o filtro de ID com o filtro de Ativos ---
            var filter = Builders<Bandeja>.Filter.Eq(m => m.Id, id) & _filtroAtivos;

            var pipeline = _context.Bandejas.Aggregate()
                .Match(filter) // <-- FILTRO ATUALIZADO
                .Lookup("Dietas", "TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Bandeja> { PreserveNullAndEmptyArrays = true });

            var bandeja = await pipeline.FirstOrDefaultAsync();

            if (bandeja == null) return NotFound();

            return View(bandeja);
        }

        // GET: Bandejas/Create
        public async Task<IActionResult> Create()
        {
            await CarregarDietasViewBag(null);
            return View();
        }

        // POST: Bandejas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodBandeja,TipoDieta")] Bandeja bandeja)
        {
            ModelState.Remove("DetalhesDieta");
            ModelState.Remove("Ativo"); // Remove da validação (o construtor já define)

            if (ModelState.IsValid)
            {
                bandeja.Id = Guid.NewGuid();
                // O construtor do Modelo já define Ativo = true
                await _context.Bandejas.InsertOneAsync(bandeja);
                return RedirectToAction(nameof(Index));
            }

            await CarregarDietasViewBag(bandeja.TipoDieta);
            return View(bandeja);
        }

        // GET: Bandejas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 5: Adiciona o filtro de Ativo ao Find ---
            var bandeja = await _context.Bandejas.Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();
            if (bandeja == null) return NotFound();

            await CarregarDietasViewBag(bandeja.TipoDieta);
            return View(bandeja);
        }

        // POST: Bandejas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CodBandeja,TipoDieta")] Bandeja bandeja)
        {
            if (id != bandeja.Id) return NotFound();

            ModelState.Remove("DetalhesDieta");
            ModelState.Remove("Ativo");

            // --- CORREÇÃO 6: Garante que a bandeja permaneça Ativa ao editar ---
            bandeja.Ativo = true;

            if (ModelState.IsValid)
            {
                try
                {
                    // Salva o objeto com Ativo = true
                    await _context.Bandejas.ReplaceOneAsync(m => m.Id == bandeja.Id, bandeja);
                }
                catch (Exception)
                {
                    if (!await BandejaExists(bandeja.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await CarregarDietasViewBag(bandeja.TipoDieta);
            return View(bandeja);
        }

        // GET: Bandejas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 7: Combina o filtro de ID com o filtro de Ativos ---
            var filter = Builders<Bandeja>.Filter.Eq(m => m.Id, id) & _filtroAtivos;

            var pipeline = _context.Bandejas.Aggregate()
                .Match(filter) // <-- FILTRO ATUALIZADO
                .Lookup("Dietas", "TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Bandeja> { PreserveNullAndEmptyArrays = true });

            var bandeja = await pipeline.FirstOrDefaultAsync();

            if (bandeja == null) return NotFound();

            return View(bandeja);
        }

        // POST: Bandejas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // --- CORREÇÃO 8: Implementa o DELETE LÓGICO ---

            var filter = Builders<Bandeja>.Filter.Eq(b => b.Id, id);
            // Define Ativo = false
            var update = Builders<Bandeja>.Update.Set(b => b.Ativo, false);

            await _context.Bandejas.UpdateOneAsync(filter, update);

            return RedirectToAction(nameof(Index));
        }

        // Método Helper
        private async Task<bool> BandejaExists(Guid id)
        {
            // --- CORREÇÃO 9: Atualiza o Helper ---
            return await _context.Bandejas.Find(e => e.Id == id & e.Ativo == true).AnyAsync();
        }
    }
}