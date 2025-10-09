using Microsoft.AspNetCore.Mvc;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class RelatorioController : Controller
    {
        public IActionResult Index()
        {
            // Uma página inicial para os relatórios, se quiser.
            return View();
        }

        // GET: Relatorio/HistoricoPaciente/5
        public IActionResult HistoricoPaciente(int pacienteId)
        {
            // TODO: Aqui vai a lógica para buscar o histórico do paciente no banco de dados.

            // Depois, você passaria os dados para a View.
            return View();
        }

        // TODO: Adicionar as outras ações aqui...
        // public IActionResult TempoMedioDieta(int copeiraId) { ... }
        // public IActionResult ErrosValidacao() { ... }
    }
}
