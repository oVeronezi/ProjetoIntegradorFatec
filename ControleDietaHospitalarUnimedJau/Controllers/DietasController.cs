using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Data;
namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class DietasController : Controller
    {
        private readonly ContextMongodb _context;

        public DietasController(ContextMongodb context)
        {
            _context = context;
        }

        // GET: Dietas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dietas.Find(_ => true).ToListAsync());
        }

        // GET: Dietas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Dieta = await _context.Dietas
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (Dieta == null)
            {
                return NotFound();
            }

            return View(Dieta);
        }

        // GET: Dietas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,NomeDieta,ItensAlimentares")] Dieta dieta)
        {
            if (ModelState.IsValid)
            {
                dieta.Id = Guid.NewGuid();
                await _context.Dietas.InsertOneAsync(dieta);
                return RedirectToAction(nameof(Index));
            }
            return View(dieta);
        }

        // GET: Dietas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieta = await _context.Dietas
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (dieta == null)
            {
                return NotFound();
            }
            return View(dieta);
        }

        // POST: Dietas/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeDieta,ItensAlimentares")] Dieta dieta)
        {
            if (id != dieta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Dietas.ReplaceOneAsync(m => m.Id == dieta.Id, dieta);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DietaExists(dieta.Id))
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
            return View(dieta);
        }

        // GET: Dietas/Delete/
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieta = await _context.Dietas.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (dieta == null)
            {
                return NotFound();
            }

            return View(dieta);
        }
        // POST: Dietas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Dietas.DeleteOneAsync(u => u.Id == id);

            return RedirectToAction(nameof(Index));
        }

        private bool DietaExists(Guid id)
        {
            return _context.Dietas.Find(e => e.Id == id).Any();
        }
    }
}
