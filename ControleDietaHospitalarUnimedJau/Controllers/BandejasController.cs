using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Data;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class BandejasController : Controller
    {
        private readonly ContextMongodb _context;

        public BandejasController(ContextMongodb context)
        {
            _context = context;
        }

        // Método helper para carregar o Dropdown de Dietas
        private async Task CarregarDietasViewBag(object? selectedDieta = null)
        {
            var dietas = await _context.Dietas.Find(_ => true)
                                           .SortBy(d => d.NomeDieta)
                                           .ToListAsync();

            ViewBag.TipoDieta = new SelectList(dietas, "Id", "NomeDieta", selectedDieta);
        }

        // GET: Bandejas (Com Lookup para mostrar o Nome da Dieta)
        public async Task<IActionResult> Index()
        {
            var pipeline = _context.Bandejas.Aggregate()
                .Lookup("Dietas", "TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Bandeja> { PreserveNullAndEmptyArrays = true });

            return View(await pipeline.ToListAsync());
        }

        // GET: Bandejas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var filter = Builders<Bandeja>.Filter.Eq(m => m.Id, id);
            var pipeline = _context.Bandejas.Aggregate()
                .Match(filter)
                .Lookup("Dietas", "TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Bandeja> { PreserveNullAndEmptyArrays = true });

            var bandeja = await pipeline.FirstOrDefaultAsync();

            if (bandeja == null) return NotFound();

            return View(bandeja);
        }

        // GET: Bandejas/Create
        public async Task<IActionResult> Create()
        {
            // Carrega o dropdown de Dietas
            await CarregarDietasViewBag(null);
            return View();
        }

        // POST: Bandejas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodBandeja,TipoDieta")] Bandeja bandeja)
        {
            ModelState.Remove("DetalhesDieta"); // Remover validação da propriedade de lookup

            if (ModelState.IsValid)
            {
                bandeja.Id = Guid.NewGuid();
                await _context.Bandejas.InsertOneAsync(bandeja);
                return RedirectToAction(nameof(Index));
            }

            // Se o ModelState falhar, recarrega o dropdown
            await CarregarDietasViewBag(bandeja.TipoDieta);
            return View(bandeja);
        }

        // GET: Bandejas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var bandeja = await _context.Bandejas.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (bandeja == null) return NotFound();

            // Carrega o dropdown e pré-seleciona a dieta atual
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

            if (ModelState.IsValid)
            {
                try
                {
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

            // Se o ModelState falhar, recarrega o dropdown
            await CarregarDietasViewBag(bandeja.TipoDieta);
            return View(bandeja);
        }

        // GET: Bandejas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var filter = Builders<Bandeja>.Filter.Eq(m => m.Id, id);
            var pipeline = _context.Bandejas.Aggregate()
                .Match(filter)
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
            await _context.Bandejas.DeleteOneAsync(m => m.Id == id);
            return RedirectToAction(nameof(Index));
        }

        // Método Helper
        private async Task<bool> BandejaExists(Guid id)
        {
            return await _context.Bandejas.Find(e => e.Id == id).AnyAsync();
        }
    }
}