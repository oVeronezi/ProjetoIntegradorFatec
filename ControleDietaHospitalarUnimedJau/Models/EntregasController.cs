using ControleDietaHospitalarUnimedJau.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class EntregasController : Controller
    {
        private readonly ContextMongodb _context;

        public EntregasController(ContextMongodb context)
        {
            _context = context;
        }

        // ==================================================================
        // GET: Entregas (LISTAGEM COM LOOKUPS MÚLTIPLOS)
        // ==================================================================
        public async Task<IActionResult> Index()
        {
            // O pipeline de agregação deve realizar 3 lookups: Paciente, Copeira e Dieta.
            var pipeline = _context.Entregas.Aggregate()

                // 1. LOOKUP Paciente
                .Lookup(
                    foreignCollection: _context.Pacientes,
                    localField: e => e.IdPaciente,
                    foreignField: p => p.Id,
                    @as: (Entrega e) => e.Paciente
                )
                .Unwind<Entrega, Entrega>(
                    e => e.Paciente,
                    new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true }
                )

                // 2. LOOKUP Copeira
                .Lookup(
                    foreignCollection: _context.Copeiras,
                    localField: e => e.IdCopeira,
                    foreignField: c => c.Id,
                    @as: (Entrega e) => e.Copeira
                )
                .Unwind<Entrega, Entrega>(
                    e => e.Copeira,
                    new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true }
                )

                // 3. LOOKUP Dieta (É Guid? e Dieta)
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: e => e.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Entrega e) => e.Dieta
                )
                .Unwind<Entrega, Entrega>(
                    e => e.Dieta,
                    new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true }
                )

                .ToListAsync();

            return View(await pipeline);
        }

        // GET: Entregas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Entrega = await _context.Entregas
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Entrega == null)
            {
                return NotFound();
            }

            return View(Entrega);
        }

        // GET: Entregas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Entregas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,NomeDieta,ItensAlimentares")] Entrega Entrega)
        {
            if (ModelState.IsValid)
            {
                Entrega.Id = Guid.NewGuid();
                await _context.Entregas.InsertOneAsync(Entrega);
                return RedirectToAction(nameof(Index));
            }
            return View(Entrega);
        }

        // GET: Entregas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Entrega = await _context.Entregas
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Entrega == null)
            {
                return NotFound();
            }
            return View(Entrega);
        }

        // POST: Entregas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeDieta,ItensAlimentares")] Entrega Entrega)
        {
            if (id != Entrega.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Entregas.ReplaceOneAsync(m => m.Id == Entrega.Id, Entrega);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntregasExists(Entrega.Id))
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
            return View(Entrega);
        }

        // GET: Entregas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Entrega = await _context.Entregas.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Entrega == null)
            {
                return NotFound();
            }

            return View(Entrega);
        }

        // POST: Entregas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Entregas.DeleteOneAsync(u => u.Id == id);

            return RedirectToAction(nameof(Index));
        }

        private bool EntregasExists(Guid id)
        {
            return _context.Entregas.Find(e => e.Id == id).Any();
        }
    }
}