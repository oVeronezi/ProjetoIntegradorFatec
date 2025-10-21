using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class EntregasController : Controller
    {
        private readonly ControleDietaHospitalarUnimedJauContext _context;

        public EntregasController(ControleDietaHospitalarUnimedJauContext context)
        {
            _context = context;
        }

        // GET: Entregas
        public async Task<IActionResult> Index()
        {
            var controleDietaHospitalarUnimedJauContext = _context.Entrega.Include(e => e.Copeira).Include(e => e.Dieta).Include(e => e.Paciente);
            return View(await controleDietaHospitalarUnimedJauContext.ToListAsync());
        }

        // GET: Entregas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrega = await _context.Entrega
                .Include(e => e.Copeira)
                .Include(e => e.Dieta)
                .Include(e => e.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entrega == null)
            {
                return NotFound();
            }

            return View(entrega);
        }

        // GET: Entregas/Create
        public IActionResult Create()
        {
            ViewData["CopeiraId"] = new SelectList(_context.Copeira, "Id", "Nome");
            ViewData["DietaId"] = new SelectList(_context.Dietas, "Id", "NomeDieta");
            ViewData["PacienteId"] = new SelectList(_context.Paciente, "Id", "CodPulseira");
            return View();
        }

        // POST: Entregas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HoraInicio,HoraFim,Status,Observacao,PacienteId,CopeiraId,DietaId")] Entrega entrega)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entrega);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CopeiraId"] = new SelectList(_context.Copeira, "Id", "Nome", entrega.CopeiraId);
            ViewData["DietaId"] = new SelectList(_context.Dietas, "Id", "NomeDieta", entrega.DietaId);
            ViewData["PacienteId"] = new SelectList(_context.Paciente, "Id", "CodPulseira", entrega.PacienteId);
            return View(entrega);
        }

        // GET: Entregas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrega = await _context.Entrega.FindAsync(id);
            if (entrega == null)
            {
                return NotFound();
            }
            ViewData["CopeiraId"] = new SelectList(_context.Copeira, "Id", "Nome", entrega.CopeiraId);
            ViewData["DietaId"] = new SelectList(_context.Dietas, "Id", "NomeDieta", entrega.DietaId);
            ViewData["PacienteId"] = new SelectList(_context.Paciente, "Id", "CodPulseira", entrega.PacienteId);
            return View(entrega);
        }

        // POST: Entregas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,HoraInicio,HoraFim,Status,Observacao,PacienteId,CopeiraId,DietaId")] Entrega entrega)
        {
            if (id != entrega.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entrega.Id);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntregaExists(entrega.Id))
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
            ViewData["CopeiraId"] = new SelectList(_context.Copeira, "Id", "Nome", entrega.CopeiraId);
            ViewData["DietaId"] = new SelectList(_context.Dietas, "Id", "NomeDieta", entrega.DietaId);
            ViewData["PacienteId"] = new SelectList(_context.Paciente, "Id", "CodPulseira", entrega.PacienteId);
            return View(entrega);
        }

        // GET: Entregas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrega = await _context.Entrega
                .Include(e => e.Copeira)
                .Include(e => e.Dieta)
                .Include(e => e.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entrega == null)
            {
                return NotFound();
            }

            return View(entrega);
        }

        // POST: Entregas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entrega = await _context.Entrega.FindAsync(id);
            if (entrega != null)
            {
                _context.Entrega.Remove(entrega);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntregaExists(Guid id)
        {
            return _context.Entrega.Any(e => e.Id == id);
        }
    }
}
