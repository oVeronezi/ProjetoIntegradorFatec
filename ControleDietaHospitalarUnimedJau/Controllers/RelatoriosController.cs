using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
// 💡 ADICIONE ESTE USING
using MongoDB.Driver.Linq;
// 💡 ADICIONE ESTE USING

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    // [Authorize(Roles = "Administrador")] // Opcional: Proteger acesso a relatórios
    public class RelatoriosController : Controller
    {
        private readonly ContextMongodb _context;

        public RelatoriosController(ContextMongodb context)
        {
            _context = context;
        }

        // ==================================================================
        // GET: Relatorios (Tela de Seleção de Relatório)
        // ==================================================================
        public async Task<IActionResult> Index()
        {
            // Carrega a lista de Copeiras para o dropdown do relatório de Tempo Médio
            ViewBag.Copeiras = new SelectList(
                await _context.Copeiras.Find(_ => true).ToListAsync(),
                "Id",
                "NomeCopeira"
            );

            // Carrega a lista de Pacientes para o dropdown do relatório de Histórico
            ViewBag.Pacientes = new SelectList(
                await _context.Pacientes.Find(_ => true).ToListAsync(),
                "Id",
                "Nome"
            );

            // Retorna a view para o usuário escolher o tipo de relatório
            return View();
        }

        // ==================================================================
        // MÉTODO AUXILIAR: Carrega todos os dados com os Lookups
        // ==================================================================
        private async Task<(List<Entrega>, List<Paciente>, List<Copeira>)> CarregarDadosRelatorioAsync()
        {
            // 1. Carrega todas as Copeiras e Pacientes (necessário para a lógica da classe Relatorio)
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();

            // 2. Pipeline para carregar Entregas com os Lookups (para a exibição na View)
            var pipelineEntregas = _context.Entregas.Aggregate()
                // LOOKUP Paciente
                .Lookup(foreignCollection: _context.Pacientes, localField: e => e.IdPaciente, foreignField: p => p.Id, @as: (Entrega e) => e.DetalhesPaciente)
                .Unwind<Entrega, Entrega>(e => e.DetalhesPaciente, new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                // LOOKUP Copeira
                .Lookup(foreignCollection: _context.Copeiras, localField: e => e.IdCopeira, foreignField: c => c.Id, @as: (Entrega e) => e.DetalhesCopeira)
                .Unwind<Entrega, Entrega>(e => e.DetalhesCopeira, new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                // LOOKUP Dieta
                .Lookup(foreignCollection: _context.Dietas, localField: e => e.IdDieta, foreignField: d => d.Id, @as: (Entrega e) => e.DetalhesDieta)
                .Unwind<Entrega, Entrega>(e => e.DetalhesDieta, new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })
                .ToListAsync();

            var entregas = await pipelineEntregas;

            return (entregas, pacientes, copeiras);
        }

        // ==================================================================
        // GET: Relatorios/Gerar/TempoMedio
        // ==================================================================
        public async Task<IActionResult> GerarTempoMedio(Guid idCopeira)
        {
            var (entregas, pacientes, copeiras) = await CarregarDadosRelatorioAsync();

            // Instancia a classe de lógica com os dados brutos
            var relatorioService = new Relatorio(entregas, pacientes, copeiras);

            // Gera o relatório específico
            var viewModel = relatorioService.GerarRelatorioTempoMedioDieta(idCopeira);

            return View("RelatorioDetalhe", viewModel);
        }

        // ==================================================================
        // GET: Relatorios/Gerar/Erros
        // ==================================================================
        public async Task<IActionResult> GerarErrosValidacao()
        {
            var (entregas, pacientes, copeiras) = await CarregarDadosRelatorioAsync();

            var relatorioService = new Relatorio(entregas, pacientes, copeiras);

            var viewModel = relatorioService.GerarRelatorioErrosValidacao();

            return View("RelatorioDetalhe", viewModel);
        }

        // ==================================================================
        // GET: Relatorios/Gerar/HistoricoPaciente
        // ==================================================================
        public async Task<IActionResult> GerarHistoricoPaciente(Guid idPaciente)
        {
            var (entregas, pacientes, copeiras) = await CarregarDadosRelatorioAsync();

            var relatorioService = new Relatorio(entregas, pacientes, copeiras);

            var viewModel = relatorioService.GerarRelatorioHistoricoPaciente(idPaciente);

            return View("RelatorioDetalhe", viewModel);
        }
    }
}