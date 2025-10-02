namespace ControleDietaHospitalarUnimedJau.Models
{
    public class RelatorioViewModel
    {
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
        }
    }
}
