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
        // --- CORREÇÃO 1: Adiciona o filtro padrão de "Ativos" ---
        private readonly FilterDefinition<Dieta> _filtroAtivos;

        public DietasController(ContextMongodb context)
        {
            _context = context;
            // Define o filtro para buscar apenas dietas ativas
            _filtroAtivos = Builders<Dieta>.Filter.Eq(d => d.Ativo, true);
        }

        // GET: Dietas
        public async Task<IActionResult> Index()
        {
            // --- CORREÇÃO 2: Usa o _filtroAtivos em vez de (_ => true) ---
            return View(await _context.Dietas.Find(_filtroAtivos).ToListAsync());
        }

        // GET: Dietas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 3: Adiciona o filtro de Ativo ao Find ---
            var Dieta = await _context.Dietas
                .Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (Dieta == null) return NotFound();

            return View(Dieta);
        }

        // GET: Dietas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dietas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Recebe 'itensString' do formulário
        public async Task<IActionResult> Create([Bind("Id,NomeDieta")] Dieta dieta, string itensString)
        {
            // 2. Converte o texto (itensString) para a Lista (ItensAlimentares)
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

            // 3. Remove validações que não vêm do formulário
            ModelState.Remove("ItensAlimentares");
            ModelState.Remove("Ativo");

            if (ModelState.IsValid)
            {
                dieta.Id = Guid.NewGuid();
                dieta.Ativo = true; // 4. Garante que nasce Ativa

                await _context.Dietas.InsertOneAsync(dieta);
                return RedirectToAction(nameof(Index));
            }

            return View(dieta);
        }

        // GET: Dietas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 4: Adiciona o filtro de Ativo ao Find ---
            var dieta = await _context.Dietas
                .Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (dieta == null) return NotFound();

            return View(dieta);
        }

        // POST: Dietas/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NomeDieta")] Dieta dieta, string itensString)
        {
            if (id != dieta.Id) return NotFound();

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

            ModelState.Remove("ItensAlimentares");
            ModelState.Remove("Ativo"); // Remove o 'Ativo' da validação

            // Define 'Ativo = true' manualmente para garantir que a edição não desative
            dieta.Ativo = true;

            if (ModelState.IsValid)
            {
                try
                {
                    // O objeto 'dieta' agora tem Ativo = true
                    await _context.Dietas.ReplaceOneAsync(m => m.Id == dieta.Id, dieta);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // --- CORREÇÃO 6 (Helper): Chamada ao DietaExists atualizado ---
                    if (!await DietaExists(dieta.Id)) // Verifica se a dieta (ativa) existe
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
            if (id == null) return NotFound();

            // --- CORREÇÃO 5: Adiciona o filtro de Ativo ao Find ---
            var dieta = await _context.Dietas.Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (dieta == null) return NotFound();

            return View(dieta);
        }

        // POST: Dietas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // --- CORREÇÃO 6: Implementa o DELETE LÓGICO ---
            // Em vez de apagar, fazemos um Update para definir Ativo = false

            var filter = Builders<Dieta>.Filter.Eq(d => d.Id, id);
            var update = Builders<Dieta>.Update.Set(d => d.Ativo, false); // Define Ativo = false

            await _context.Dietas.UpdateOneAsync(filter, update);

            return RedirectToAction(nameof(Index));
        }

        // --- CORREÇÃO 7: Atualiza o Helper para verificar apenas Ativos ---
        private async Task<bool> DietaExists(Guid id)
        {
            return await _context.Dietas.Find(e => e.Id == id & e.Ativo == true).AnyAsync();
        }
    }
}