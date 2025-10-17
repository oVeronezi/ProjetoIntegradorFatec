using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class CopeirasController : Controller
    {
        private readonly ContextMongodb _context;

        public CopeirasController(ContextMongodb context)
        {
            _context = context;
        }

        // GET: Copeiras
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dietas.Find(_ => true).ToListAsync());
        }

        // GET: Copeiras/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeiras
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (copeira == null)
            {
                return NotFound();
            }

            return View(copeira);
        }

        // GET: Copeiras/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Copeiras/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
 
        public async Task<IActionResult> Create([Bind("Id,Nome")] Copeira copeira)
        {
            if (ModelState.IsValid)
            {
                copeira.Id = Guid.NewGuid();
                await _context.Copeiras.InsertOneAsync(copeira);
                return RedirectToAction(nameof(Index));
            }
            return View(copeira);
        }


        // GET: Copeiras/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeiras.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (copeira == null)
            {
                return NotFound();
            }
            return View(copeira);
        }


        // POST: Copeiras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome")] Copeira copeira)
        {
            if (id != copeira.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Copeiras.ReplaceOneAsync(m => m.Id == copeira.Id, copeira);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CopeiraExists(copeira.Id))
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
            return View(copeira);
        }
    


        // GET: Copeiras/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeiras
                .Find(m => m.Id == id).FirstOrDefaultAsync();
            if (copeira == null)
            {
                return NotFound();
            }

            return View(copeira);
        }

        // POST: Copeiras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Copeiras.DeleteOneAsync(u => u.Id == id);

            return RedirectToAction(nameof(Index));
        }

        private bool CopeiraExists(Guid id)
        {
            return _context.Copeiras.Find(e => e.Id == id).Any();
        }
    }
}
