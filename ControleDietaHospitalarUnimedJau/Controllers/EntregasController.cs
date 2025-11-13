using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Data;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class EntregasController : Controller
    {
        private readonly ContextMongodb _context;

        public EntregasController(ContextMongodb context)
        {
            _context = context;
        }

        // GET: Index (Correto! O Lookup não filtra, mantendo o histórico)
        public async Task<IActionResult> Index()
        {
            var pipeline = _context.Entregas.Aggregate()
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            var entregasCompletas = await pipeline.ToListAsync();

            return View(entregasCompletas);
        }

        // GET: Details (Correto! O Lookup não filtra, mantendo o histórico)
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var filter = Builders<Entrega>.Filter.Eq(m => m.Id, id);

            var pipeline = _context.Entregas.Aggregate()
                .Match(filter)
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            var entrega = await pipeline.FirstOrDefaultAsync();

            if (entrega == null) return NotFound();

            return View(entrega);
        }

        // GET: Entregas/Create
        public async Task<IActionResult> Create()
        {
            // --- CORREÇÃO 1: Filtrar Dropdowns para mostrar apenas ATIVOS ---
            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome");

            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome");

            // (Vamos filtrar Bandejas também, antecipando o próximo passo)
            var bandejas = await _context.Bandejas.Find(b => b.Ativo == true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja");

            return View();
        }

        // POST: Entregas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Observacao,IdPaciente,IdCopeira,IdBandeja")] Entrega entrega)
        {
            // ... (Remoção dos ModelStates - está correto) ...
            ModelState.Remove("DetalhesPaciente");
            ModelState.Remove("DetalhesCopeira");
            ModelState.Remove("DetalhesBandeja");
            ModelState.Remove("DetalhesDieta");
            ModelState.Remove("HoraInicio");
            ModelState.Remove("HoraFim");
            ModelState.Remove("StatusValidacao");

            if (ModelState.IsValid)
            {
                entrega.Id = Guid.NewGuid();
                entrega.HoraInicio = DateTime.UtcNow;
                entrega.HoraFim = null;
                entrega.StatusValidacao = "Em andamento";
                await _context.Entregas.InsertOneAsync(entrega);
                return RedirectToAction(nameof(Index));
            }

            // --- CORREÇÃO 2: Filtrar Dropdowns (em caso de erro de validação) ---
            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(b => b.Ativo == true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // GET: Entregas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            // (Não filtramos por Ativo aqui, pois podemos querer editar uma entrega antiga)
            var entrega = await _context.Entregas.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (entrega == null) return NotFound();

            // --- CORREÇÃO 3: Filtrar Dropdowns para mostrar apenas ATIVOS ---
            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(b => b.Ativo == true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // POST: Entregas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,HoraInicio,HoraFim,StatusValidacao,Observacao,IdPaciente,IdCopeira,IdBandeja")] Entrega entrega)
        {
            if (id != entrega.Id) return NotFound();

            // ... (Remoção dos ModelStates - está correto) ...
            ModelState.Remove("DetalhesPaciente");
            ModelState.Remove("DetalhesCopeira");
            ModelState.Remove("DetalhesBandeja");
            ModelState.Remove("DetalhesDieta");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Entregas.ReplaceOneAsync(m => m.Id == entrega.Id, entrega);
                }
                catch (Exception)
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

            // --- CORREÇÃO 4: Filtrar Dropdowns (em caso de erro de validação) ---
            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(b => b.Ativo == true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // GET: Delete (Correto! Não filtra por Ativo)
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var filter = Builders<Entrega>.Filter.Eq(m => m.Id, id);
            var pipeline = _context.Entregas.Aggregate()
                .Match(filter)
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            var entrega = await pipeline.FirstOrDefaultAsync();

            if (entrega == null) return NotFound();

            return View(entrega);
        }

        // POST: Delete (Físico - Correto)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // O Delete de Entregas é físico (Delete Lógico não se aplica aqui)
            await _context.Entregas.DeleteOneAsync(m => m.Id == id);
            return RedirectToAction(nameof(Index));
        }

        private bool EntregaExists(Guid id)
        {
            return _context.Entregas.Find(e => e.Id == id).Any();
        }

        // POST: Entregas/Finalizar/5 (Correto)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(Guid id)
        {
            var entrega = await _context.Entregas.Find(m => m.Id == id).FirstOrDefaultAsync();

            if (entrega != null && !entrega.HoraFim.HasValue)
            {
                entrega.HoraFim = DateTime.UtcNow;
                entrega.StatusValidacao = "Concluído";
                await _context.Entregas.ReplaceOneAsync(m => m.Id == entrega.Id, entrega);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}