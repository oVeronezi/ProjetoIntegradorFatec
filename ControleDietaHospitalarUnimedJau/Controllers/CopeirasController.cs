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
using Microsoft.AspNetCore.Authorization; // <--- Necessário para a segurança

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    [Authorize]
    public class CopeirasController : Controller
    {
        private readonly ContextMongodb _context;
        // --- CORREÇÃO 1: Adiciona o filtro padrão de "Ativos" ---
        private readonly FilterDefinition<Copeira> _filtroAtivos;

        public CopeirasController(ContextMongodb context)
        {
            _context = context;
            // Define o filtro para buscar apenas copeiras ativas
            _filtroAtivos = Builders<Copeira>.Filter.Eq(c => c.Ativo, true);
        }

        // GET: Copeiras
        public async Task<IActionResult> Index()
        {
            // --- CORREÇÃO 2: Usa o _filtroAtivos ---
            return View(await _context.Copeiras.Find(_filtroAtivos).ToListAsync());
        }

        // GET: Copeiras/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 3: Adiciona o filtro de Ativo ao Find ---
            var copeira = await _context.Copeiras
                .Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (copeira == null) return NotFound();

            return View(copeira);
        }

        // GET: Copeiras/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Copeiras/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Copeira copeira)
        {
            ModelState.Remove("Ativo"); // Remove o 'Ativo' da validação

            if (ModelState.IsValid)
            {
                copeira.Id = Guid.NewGuid();
                // --- CORREÇÃO 4: Garante que a nova copeira é Ativa ---
                // (O construtor que fizemos no Modelo já faz isto, 
                // mas é uma boa prática garantir)
                copeira.Ativo = true;

                await _context.Copeiras.InsertOneAsync(copeira);
                return RedirectToAction(nameof(Index));
            }
            return View(copeira);
        }


        // GET: Copeiras/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO 5: Adiciona o filtro de Ativo ao Find ---
            var copeira = await _context.Copeiras.Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (copeira == null) return NotFound();

            return View(copeira);
        }

        // POST: Copeiras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome")] Copeira copeira)
        {
            if (id != copeira.Id) return NotFound();

            ModelState.Remove("Ativo");

            // --- CORREÇÃO 6: Garante que a edição não desative a copeira ---
            // O [Bind] só traz Id e Nome, então "Ativo" viria como 'false' (padrão)
            // Nós definimos manualmente como 'true' antes de salvar.
            copeira.Ativo = true;

            if (ModelState.IsValid)
            {
                try
                {
                    // Agora o 'ReplaceOneAsync' salva o objeto com Ativo = true
                    await _context.Copeiras.ReplaceOneAsync(m => m.Id == copeira.Id, copeira);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CopeiraExists(copeira.Id)) // Chamada ao helper atualizado
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
            if (id == null) return NotFound();

            // --- CORREÇÃO 7: Adiciona o filtro de Ativo ao Find ---
            var copeira = await _context.Copeiras
                .Find(m => m.Id == id & m.Ativo == true).FirstOrDefaultAsync();

            if (copeira == null) return NotFound();

            return View(copeira);
        }

        // POST: Copeiras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // --- CORREÇÃO 8: Implementa o DELETE LÓGICO ---

            var filter = Builders<Copeira>.Filter.Eq(c => c.Id, id);
            // Define Ativo = false
            var update = Builders<Copeira>.Update.Set(c => c.Ativo, false);

            await _context.Copeiras.UpdateOneAsync(filter, update);

            return RedirectToAction(nameof(Index));
        }

        // --- CORREÇÃO 9: Atualiza o Helper ---
        private async Task<bool> CopeiraExists(Guid id)
        {
            // Verifica apenas copeiras ativas
            return await _context.Copeiras.Find(e => e.Id == id & e.Ativo == true).AnyAsync();
        }
    }
}