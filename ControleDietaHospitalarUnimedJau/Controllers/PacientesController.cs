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

        // ------------------------------------------------------------------
        // GET: Pacientes (LISTAGEM COM DIETA VINCULADA)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // O pipeline de agregação com $lookup está CORRETO e carrega a DietaVinculada.
            var pipeline = _context.Pacientes.Aggregate()
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DietaVinculada
                )
                .Unwind<Paciente, Paciente>(
                     p => p.DietaVinculada,
                     new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                )
                .ToListAsync();

            return View(await pipeline);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Details/5 (CARREGA PACIENTE + DIETA)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Para mostrar os detalhes da dieta, o Details também precisa do lookup.
            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id) // Filtra pelo ID
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DietaVinculada
                )
                .Unwind<Paciente, Paciente>(
                     p => p.DietaVinculada,
                     new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                );

            var Paciente = await pipeline.FirstOrDefaultAsync();

            if (Paciente == null)
            {
                return NotFound();
            }

            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Create (PREPARA FORMULÁRIO)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            // Adiciona a lista de Dietas para o dropdown na View
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id", // Valor que será armazenado (o Guid da Dieta)
                "NomeDieta" // Texto que será exibido no dropdown
            );
            return View();
        }

        // ------------------------------------------------------------------
        // POST: Pacientes/Create
        // ------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken] // Boa prática de segurança
        // ATENÇÃO: O Bind deve listar TODAS as propriedades do Paciente
        // e incluir o IdDieta
        public async Task<IActionResult> Create([Bind("Nome,NumQuarto,CodPulseira,IdDieta")] Paciente Paciente)
        {
            if (ModelState.IsValid)
            {
                Paciente.Id = Guid.NewGuid();
                await _context.Pacientes.InsertOneAsync(Paciente);
                return RedirectToAction(nameof(Index));
            }

            // Recarrega o dropdown em caso de erro de validação
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta // Seleciona o ID que o usuário tentou enviar
            );
            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Edit/5 (CARREGA FORMULÁRIO E DIETAS)
        // ------------------------------------------------------------------
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca o paciente apenas para edição
            var Paciente = await _context.Pacientes.Find(m => m.Id == id).FirstOrDefaultAsync();

            if (Paciente == null)
            {
                return NotFound();
            }

            // Carrega a lista de Dietas para o dropdown
            // O último parâmetro (Paciente.IdDieta) seleciona a dieta atual do paciente.
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );

            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // POST: Pacientes/Edit/5
        // ------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken] // Boa prática de segurança
        // ATENÇÃO: O Bind deve listar TODAS as propriedades do Paciente
        // e o IdDieta. O 'Id' já está no método.
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome,NumQuarto,CodPulseira,IdDieta")] Paciente Paciente)
        {
            if (id != Paciente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Usamos o ReplaceOneAsync para atualizar todo o documento
                var result = await _context.Pacientes.ReplaceOneAsync(m => m.Id == Paciente.Id, Paciente);

                // O ReplaceOneAsync não lança exceção de concorrência como o EF Core,
                // então verificamos se o documento realmente existia.
                if (result.MatchedCount == 0)
                {
                    return NotFound(); // Documento não encontrado para substituição
                }

                return RedirectToAction(nameof(Index));
            }

            // Recarrega o dropdown em caso de erro de validação
            ViewBag.IdDieta = new SelectList(
                await _context.Dietas.Find(_ => true).ToListAsync(),
                "Id",
                "NomeDieta",
                Paciente.IdDieta
            );
            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // GET: Pacientes/Delete/5
        // ------------------------------------------------------------------
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // O Delete e o Details precisam do lookup para exibir os dados corretamente.
            var pipeline = _context.Pacientes.Aggregate()
                .Match(p => p.Id == id)
                .Lookup(
                    foreignCollection: _context.Dietas,
                    localField: p => p.IdDieta,
                    foreignField: d => d.Id,
                    @as: (Paciente p) => p.DietaVinculada
                )
                .Unwind<Paciente, Paciente>(
                     p => p.DietaVinculada,
                     new AggregateUnwindOptions<Paciente> { PreserveNullAndEmptyArrays = true }
                );

            var Paciente = await pipeline.FirstOrDefaultAsync();

            if (Paciente == null)
            {
                return NotFound();
            }

            return View(Paciente);
        }

        // ------------------------------------------------------------------
        // POST: Pacientes/Delete/5
        // ------------------------------------------------------------------
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