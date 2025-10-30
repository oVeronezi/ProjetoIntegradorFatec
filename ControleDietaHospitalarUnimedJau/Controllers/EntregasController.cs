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

        // GET: Index (Completo, com Lookups)
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

        // GET: Details (Completo, com Lookups)
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filter = Builders<Entrega>.Filter.Eq(m => m.Id, id);

            var pipeline = _context.Entregas.Aggregate()
                .Match(filter) // <-- Apenas este ID
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            var entrega = await pipeline.FirstOrDefaultAsync();

            if (entrega == null)
            {
                return NotFound();
            }

            return View(entrega);
        }

        // GET: Entregas/Create (Completo)
        public async Task<IActionResult> Create()
        {
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome");

            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome");

            var bandejas = await _context.Bandejas.Find(_ => true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja");

            return View();
        }

        // POST: Entregas/Create (Corrigido para Status Automático)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // --- CORREÇÃO 1: Removido "StatusValidacao" do [Bind] ---
        public async Task<IActionResult> Create([Bind("Observacao,IdPaciente,IdCopeira,IdBandeja")] Entrega entrega)
        {
            ModelState.Remove("DetalhesPaciente");
            ModelState.Remove("DetalhesCopeira");
            ModelState.Remove("DetalhesBandeja");
            ModelState.Remove("DetalhesDieta");
            ModelState.Remove("HoraInicio");
            ModelState.Remove("HoraFim");
            // --- CORREÇÃO 2: Removemos o Status da validação ---
            ModelState.Remove("StatusValidacao");

            if (ModelState.IsValid)
            {
                entrega.Id = Guid.NewGuid();
                entrega.HoraInicio = DateTime.UtcNow;
                entrega.HoraFim = null;
                // --- CORREÇÃO 3: Definir o Status automaticamente ---
                entrega.StatusValidacao = "Em andamento";

                await _context.Entregas.InsertOneAsync(entrega);
                return RedirectToAction(nameof(Index));
            }

            // Se a validação falhar, recarrega os ViewBags (código existente)
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(_ => true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // GET: Entregas/Edit/5 (Completo)
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrega = await _context.Entregas.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (entrega == null)
            {
                return NotFound();
            }

            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(_ => true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // POST: Entregas/Edit/5 (Completo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,HoraInicio,HoraFim,StatusValidacao,Observacao,IdPaciente,IdCopeira,IdBandeja")] Entrega entrega)
        {
            if (id != entrega.Id)
            {
                return NotFound();
            }

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
                    // ----- CORREÇÃO -----
                    // É aqui (linha 171 do teu erro) que o "EntregaExists" é chamado.
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

            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            ViewBag.IdPaciente = new SelectList(pacientes, "Id", "Nome", entrega.IdPaciente);

            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();
            ViewBag.IdCopeira = new SelectList(copeiras, "Id", "Nome", entrega.IdCopeira);

            var bandejas = await _context.Bandejas.Find(_ => true).ToListAsync();
            ViewBag.IdBandeja = new SelectList(bandejas, "Id", "CodBandeja", entrega.IdBandeja);

            return View(entrega);
        }

        // GET: Delete (Completo, com Lookups)
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // O erro CS0161 (linha 200) era um erro em cascata.
            // Este código está correto.
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

            if (entrega == null)
            {
                return NotFound();
            }

            return View(entrega);
        }

        // POST: Delete (Completo)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _context.Entregas.DeleteOneAsync(m => m.Id == id);
            return RedirectToAction(nameof(Index));
        }

        // ----- CORREÇÃO (O MÉTODO QUE FALTAVA) -----
        // Este é o método que estava a causar o erro CS0103.
        // Ele deve estar dentro da classe EntregasController,
        // no final.
        private bool EntregaExists(Guid id)
        {
            return _context.Entregas.Find(e => e.Id == id).Any();
        }

        // ----- INÍCIO DO NOVO MÉTODO "FINALIZAR" -----

        // POST: Entregas/Finalizar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(Guid id)
        {
            // 1. Encontra a entrega no banco de dados
            var entrega = await _context.Entregas.Find(m => m.Id == id).FirstOrDefaultAsync();

            // 2. Verifica se ela existe e se ainda não foi finalizada
            if (entrega != null && !entrega.HoraFim.HasValue)
            {
                // 3. Define a HoraFim e o novo Status
                entrega.HoraFim = DateTime.UtcNow;
                entrega.StatusValidacao = "Concluído"; // Ou "Entregue"

                // 4. Salva a atualização no banco
                await _context.Entregas.ReplaceOneAsync(m => m.Id == entrega.Id, entrega);
            }

            // 5. Retorna para a página Index
            return RedirectToAction(nameof(Index));
        }// ----- FIM DO MÉTODO "FINALIZAR" -----
    }
}