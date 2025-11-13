using System.Collections.Generic; // Garanta que este 'using' está presente

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class RelatorioViewModel
    {
        // ✅ NOVA PROPRIEDADE!
        // Esta chave será usada pela View para a lógica (ex: "TempoMedio", "Erros")
        public string ChaveTipoRelatorio { get; set; }

        // Esta propriedade fica apenas para o título de exibição
        public string TipoRelatorio { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public List<Entrega> Entregas { get; set; }
        public Dictionary<string, double> TemposMedios { get; set; }
        public int TotalErros { get; set; }

        public RelatorioViewModel()
        {
            Entregas = new List<Entrega>();
            TemposMedios = new Dictionary<string, double>();
            ChaveTipoRelatorio = "Indefinido"; // Valor padrão
        }
    }
}