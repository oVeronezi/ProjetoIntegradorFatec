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
    public class EntregasController : Controller
    {
        private readonly ContextMongodb _context;

        public EntregasController(ContextMongodb context)
        {
            _context = context;
        }

        // GET: Entregas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Entregas.Find(_ => true).ToListAsync());
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
