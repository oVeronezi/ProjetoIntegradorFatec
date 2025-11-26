using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Data;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Authorization;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly ContextMongodb _context;

        public RelatoriosController(ContextMongodb context)
        {
            _context = context;
        }

        // 1. MENU PRINCIPAL
        public async Task<IActionResult> Index()
        {
            var copeiras = await _context.Copeiras.Find(c => c.Ativo == true).ToListAsync();
            ViewBag.Copeiras = new SelectList(copeiras, "Id", "Nome");

            var pacientes = await _context.Pacientes.Find(p => p.Ativo == true).ToListAsync();
            ViewBag.Pacientes = new SelectList(pacientes, "Id", "Nome");

            return View();
        }

        // ============================================================
        // RELATÓRIO 1: TEMPO MÉDIO (Tela + PDF)
        // ============================================================

        // Tela (HTML)
        [HttpGet]
        public async Task<IActionResult> GerarTempoMedio(Guid idCopeira)
        {
            var viewModel = await ConstruirViewModelTempoMedio(idCopeira);
            // USA O CAMINHO EXPLÍCITO PARA EVITAR ERRO 404
            return View("~/Views/Relatorios/TempoMedio.cshtml", viewModel);
        }

        // PDF
        [HttpGet]
        public async Task<IActionResult> GerarTempoMedioPdf(Guid idCopeira)
        {
            var viewModel = await ConstruirViewModelTempoMedio(idCopeira);

            // USA O CAMINHO EXPLÍCITO PARA EVITAR ERRO 404
            return new ViewAsPdf("~/Views/Relatorios/TempoMedio.cshtml", viewModel)
            {
                FileName = $"Relatorio_Copeira_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access --disable-smart-shrinking"
            };
        }

        // ============================================================
        // RELATÓRIO 2: HISTÓRICO PACIENTE (Tela + PDF)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GerarHistoricoPaciente(Guid idPaciente)
        {
            var viewModel = await ConstruirViewModelHistorico(idPaciente);
            // CAMINHO EXPLÍCITO
            return View("~/Views/Relatorios/HistoricoPaciente.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GerarHistoricoPacientePdf(Guid idPaciente)
        {
            var viewModel = await ConstruirViewModelHistorico(idPaciente);
            // CAMINHO EXPLÍCITO
            return new ViewAsPdf("~/Views/Relatorios/HistoricoPaciente.cshtml", viewModel)
            {
                FileName = $"Historico_Paciente_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access --disable-smart-shrinking"
            };
        }

        // ============================================================
        // RELATÓRIO 3: ERROS DE VALIDAÇÃO (Tela + PDF)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GerarErrosValidacao()
        {
            var viewModel = await ConstruirViewModelErros();
            return View("ErrosValidacao", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GerarErrosValidacaoPdf()
        {
            var viewModel = await ConstruirViewModelErros();
            return new ViewAsPdf("ErrosValidacao", viewModel)
            {
                FileName = $"Erros_Validacao_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access --disable-smart-shrinking"
            };
        }

        // ============================================================
        // MÉTODOS AUXILIARES (Necessários para tudo funcionar)
        // ============================================================

        private async Task<RelatorioViewModel> ConstruirViewModelTempoMedio(Guid idCopeira)
        {
            var dados = await CarregarDadosCompletosAsync();
            var servicoRelatorio = new Relatorio(dados.Entregas, dados.Pacientes, dados.Copeiras);
            var viewModel = servicoRelatorio.GerarRelatorioTempoMedioDieta(idCopeira);
            viewModel.ChaveTipoRelatorio = "TempoMedio";
            return viewModel;
        }

        private async Task<RelatorioViewModel> ConstruirViewModelHistorico(Guid idPaciente)
        {
            var dados = await CarregarDadosCompletosAsync();
            var servicoRelatorio = new Relatorio(dados.Entregas, dados.Pacientes, dados.Copeiras);
            var viewModel = servicoRelatorio.GerarRelatorioHistoricoPaciente(idPaciente);
            viewModel.ChaveTipoRelatorio = "HistoricoPaciente";
            return viewModel;
        }

        private async Task<RelatorioViewModel> ConstruirViewModelErros()
        {
            var dados = await CarregarDadosCompletosAsync();
            var servicoRelatorio = new Relatorio(dados.Entregas, dados.Pacientes, dados.Copeiras);
            var viewModel = servicoRelatorio.GerarRelatorioErrosValidacao();
            viewModel.ChaveTipoRelatorio = "ErrosValidacao";
            return viewModel;
        }

        // Carrega todos os dados do banco de uma vez
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
    }
}