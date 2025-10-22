using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class PacientesController : Controller
    {
        private readonly ContextMongodb _context;

        public PacientesController(ContextMongodb context)
        {
            _context = context;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pacientes.Find(_ => true).ToListAsync());
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Paciente = await _context.Pacientes
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Paciente == null)
            {
                return NotFound();
            }

            return View(Paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,NomeDieta,ItensAlimentares")] Paciente Paciente)
        {
            if (ModelState.IsValid)
            {
                Paciente.Id = Guid.NewGuid();
                await _context.Pacientes.InsertOneAsync(Paciente);
                return RedirectToAction(nameof(Index));
            }
            return View(Paciente);
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Paciente = await _context.Pacientes
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Paciente == null)
            {
                return NotFound();
            }
            return View(Paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeDieta,ItensAlimentares")] Paciente Paciente)
        {
            if (id != Paciente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Pacientes.ReplaceOneAsync(m => m.Id == Paciente.Id, Paciente);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacientesExists(Paciente.Id))
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
            return View(Paciente);
        }

        // GET: Pacientes/Delete/
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Paciente = await _context.Pacientes.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Paciente == null)
            {
                return NotFound();
            }

            return View(Paciente);
        }

        // POST: Dietas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Pacientes.DeleteOneAsync(u => u.Id == id);

            return RedirectToAction(nameof(Index));
        }

        private bool PacientesExists(Guid id)
        {
            return _context.Pacientes.Find(e => e.Id == id).Any();
        }
    }
}
