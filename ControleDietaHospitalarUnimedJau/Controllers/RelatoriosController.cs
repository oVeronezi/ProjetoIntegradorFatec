using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Data;
using Rotativa.AspNetCore; // Biblioteca de PDF

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly ContextMongodb _context;

        public RelatoriosController(ContextMongodb context)
        {
            _context = context;
        }

        // ============================================================
        // 1. MENU PRINCIPAL
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // Carrega filtros para a View Index
            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.Copeiras = new SelectList(copeiras, "Id", "Nome");

            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.Pacientes = new SelectList(pacientes, "Id", "Nome");

            return View();
        }

        // ============================================================
        // 2. RELATÓRIO NA TELA (HTML)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GerarTempoMedio(Guid idCopeira)
        {
            var viewModel = await ConstruirViewModelTempoMedio(idCopeira);
            // No método GerarTempoMedio (HTML)
            return View("~/Views/Relatorios/TempoMedio.cshtml", viewModel);

            // No método GerarTempoMedioPdf (PDF)
            return new ViewAsPdf("~/Views/Relatorios/TempoMedio.cshtml", viewModel);
        }

        // ============================================================
        // 3. RELATÓRIO EM PDF (Ação de Download)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GerarTempoMedioPdf(Guid idCopeira)
        {
            var viewModel = await ConstruirViewModelTempoMedio(idCopeira);

            // Retorna a mesma View "TempoMedio", mas convertida em PDF
            return new ViewAsPdf("TempoMedio", viewModel)
            {
                FileName = $"Relatorio_Copeira_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access --disable-smart-shrinking"
            };
        }

        // ============================================================
        // MÉTODOS AUXILIARES (Helpers)
        // ============================================================

        // Constrói o objeto de dados para o relatório
        private async Task<RelatorioViewModel> ConstruirViewModelTempoMedio(Guid idCopeira)
        {
            // Chama o método que carrega tudo do banco (O QUE ESTAVA FALTANDO)
            var dados = await CarregarDadosCompletosAsync();

            var servicoRelatorio = new Relatorio(dados.Entregas, dados.Pacientes, dados.Copeiras);
            var viewModel = servicoRelatorio.GerarRelatorioTempoMedioDieta(idCopeira);

            viewModel.ChaveTipoRelatorio = "TempoMedio";

            return viewModel;
        }

        // Carrega todas as tabelas necessárias com os Lookups
        private async Task<(List<Entrega> Entregas, List<Paciente> Pacientes, List<Copeira> Copeiras)> CarregarDadosCompletosAsync()
        {
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();

            var pipeline = _context.Entregas.Aggregate()
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            var entregas = await pipeline.ToListAsync();

            return (entregas, pacientes, copeiras);
        }

        // Placeholders para evitar erros nos outros botões do menu Index
        public IActionResult GerarErrosValidacao() { return Content("Em construção..."); }
        public IActionResult GerarHistoricoPaciente(Guid idPaciente) { return Content("Em construção..."); }
    }
}