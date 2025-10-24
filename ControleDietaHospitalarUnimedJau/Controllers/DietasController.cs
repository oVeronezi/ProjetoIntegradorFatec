using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Este using parece não ser necessário para MongoDB
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
        [HttpPost]
        // --- ALTERAÇÃO 1 (CREATE) ---
        // Removido "ItensAlimentares" do [Bind]
        // Adicionado "string itensString" como parâmetro
        public async Task<IActionResult> Create([Bind("Id,NomeDieta")] Dieta dieta, string itensString)
        {
            // --- ALTERAÇÃO 2 (CREATE) ---
            // Adicionámos a lógica para converter a string em lista
            if (!string.IsNullOrEmpty(itensString))
            {
                dieta.ItensAlimentares = itensString.Split('\n')
                                                  .Select(s => s.Trim())
                                                  .Where(s => !string.IsNullOrEmpty(s))
                                                  .ToList();
            }
            else
            {
                dieta.ItensAlimentares = new List<string>();
            }

            // --- ALTERAÇÃO 3 (CREATE) ---
            // Removemos a validação do campo antigo
            ModelState.Remove("ItensAlimentares");

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
        // --- ALTERAÇÃO 1 (EDIT) ---
        // Removido "ItensAlimentares" do [Bind]
        // Adicionado "string itensString" como parâmetro
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeDieta")] Dieta dieta, string itensString)
        {
            if (id != dieta.Id)
            {
                return NotFound();
            }

            // --- ALTERAÇÃO 2 (EDIT) ---
            // Adicionámos a lógica para converter a string em lista
            if (!string.IsNullOrEmpty(itensString))
            {
                dieta.ItensAlimentares = itensString.Split('\n') // Divide por quebra de linha
                                                  .Select(s => s.Trim()) // Remove espaços em branco
                                                  .Where(s => !string.IsNullOrEmpty(s)) // Remove linhas vazias
                                                  .ToList(); // Converte para Lista
            }
            else
            {
                // Se a caixa de texto estava vazia, define a lista como vazia
                dieta.ItensAlimentares = new List<string>();
            }

            // --- ALTERAÇÃO 3 (EDIT) ---
            // Removemos a validação do campo antigo, pois tratámos dele manualmente
            ModelState.Remove("ItensAlimentares");


            if (ModelState.IsValid)
            {
                try
                {
                    // Agora o objeto "dieta" está completo e será salvo corretamente
                    await _context.Dietas.ReplaceOneAsync(m => m.Id == dieta.Id, dieta);
                }
                catch (DbUpdateConcurrencyException) // Nota: DbUpdateConcurrencyException é do EntityFramework, não do Mongo.
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
        [Authorize(Roles = "Administrador")] // Certifique-se de que tem esta Role configurada
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