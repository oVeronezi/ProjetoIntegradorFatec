using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
// using Microsoft.AspNetCore.Authorization; // Descomente se for usar [Authorize]

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
        // 💡 CORREÇÃO AQUI: Usando LINQ e Where para garantir que o Id não é null.
        // ==================================================================
        public async Task<IActionResult> Index()
        {
            // --- Carrega a lista de Copeiras para o dropdown ----------------------
            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();

            // Garantimos que o ID é válido e mapeamos para SelectListItem.
            ViewBag.Copeiras = copeiras
                .Where(c => c.Id != null) // Filtra IDs nulos
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToList();

            // --- Carrega a lista de Pacientes para o dropdown ---------------------
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();

            // Garantimos que o ID é válido e mapeamos para SelectListItem.
            ViewBag.Pacientes = pacientes
                .Where(p => p.Id != null) // Filtra IDs nulos
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nome
                })
                .ToList();

            // Retorna a view para o usuário escolher o tipo de relatório
            return View();
        }

        // ==================================================================
        // MÉTODO AUXILIAR: Carrega todos os dados com os Lookups
        // ==================================================================
        // ----- INÍCIO DO MÉTODO CORRIGIDO -----

        private async Task<(List<Entrega>, List<Paciente>, List<Copeira>)> CarregarDadosRelatorioAsync()
        {
            // 1. Carrega todas as Copeiras e Pacientes (necessário para a lógica da classe Relatorio)
            var pacientes = await _context.Pacientes.Find(_ => true).ToListAsync();
            var copeiras = await _context.Copeiras.Find(_ => true).ToListAsync();

            // 2. Pipeline para carregar Entregas com os Lookups (para a exibição na View)

            // ----- CORREÇÃO -----
            // Substituímos o pipeline antigo (que usava o "IdDieta" inexistente)
            // pelo novo pipeline (que usa a Bandeja para encontrar a Dieta).
            // Esta é a mesma lógica que usamos no EntregasController.

            var pipeline = _context.Entregas.Aggregate()
                // $lookup Paciente
                .Lookup("Pacientes", "IdPaciente", "_id", "DetalhesPaciente")
                .Unwind("DetalhesPaciente", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })

                // $lookup Copeira
                .Lookup("Copeiras", "IdCopeira", "_id", "DetalhesCopeira")
                .Unwind("DetalhesCopeira", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })

                // $lookup Bandeja (Etapa 1)
                .Lookup("Bandejas", "IdBandeja", "_id", "DetalhesBandeja")
                .Unwind("DetalhesBandeja", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true })

                // $lookup Dieta (Etapa 2 - Através da Bandeja)
                .Lookup("Dietas", "DetalhesBandeja.TipoDieta", "_id", "DetalhesDieta")
                .Unwind("DetalhesDieta", new AggregateUnwindOptions<Entrega> { PreserveNullAndEmptyArrays = true });

            // O "pipeline.ToListAsync()" executa a consulta
            var entregas = await pipeline.ToListAsync();

            return (entregas, pacientes, copeiras);
        }

        // ----- FIM DO MÉTODO CORRIGIDO -----

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