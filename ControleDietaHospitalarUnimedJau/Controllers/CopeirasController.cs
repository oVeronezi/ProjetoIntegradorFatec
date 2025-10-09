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
    public class CopeirasController : Controller
    {
        private readonly ControleDietaHospitalarUnimedJauContext _context;

        public CopeirasController(ControleDietaHospitalarUnimedJauContext context)
        {
            _context = context;
        }

        // GET: Copeiras
        public async Task<IActionResult> Index()
        {
            return View(await _context.Copeira.ToListAsync());
        }

        // GET: Copeiras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeira
                .FirstOrDefaultAsync(m => m.Id == id);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Copeira copeira)
        {
            if (ModelState.IsValid)
            {
                _context.Add(copeira);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(copeira);
        }

        // GET: Copeiras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeira.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Copeira copeira)
        {
            if (id != copeira.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(copeira);
                    await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var copeira = await _context.Copeira
                .FirstOrDefaultAsync(m => m.Id == id);
            if (copeira == null)
            {
                return NotFound();
            }

            return View(copeira);
        }

        // POST: Copeiras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var copeira = await _context.Copeira.FindAsync(id);
            if (copeira != null)
            {
                _context.Copeira.Remove(copeira);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CopeiraExists(int id)
        {
            return _context.Copeira.Any(e => e.Id == id);
        }
    }
}
