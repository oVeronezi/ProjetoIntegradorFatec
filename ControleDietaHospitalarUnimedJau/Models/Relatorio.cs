using ControleDietaHospitalarUnimedJau.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControleDietaHospitalarUnimedJau.Models
{
    // A CLASSE COMEÇA AQUI (Isto foi o que faltou)
    public class Relatorio
    {
        // 1. Definição das variáveis (Campos)
        private readonly List<Entrega> _entregas;
        private readonly List<Paciente> _pacientes;
        private readonly List<Copeira> _copeiras;

        // 2. Construtor (Preenche as variáveis)
        public Relatorio(List<Entrega> entregas, List<Paciente> pacientes, List<Copeira> copeiras)
        {
            _entregas = entregas ?? new List<Entrega>();
            _pacientes = pacientes ?? new List<Paciente>();
            _copeiras = copeiras ?? new List<Copeira>();
        }

        // 3. Métodos (A lógica)

        public RelatorioViewModel GerarRelatorioTempoMedioDieta(Guid idCopeira)
        {
            var copeira = _copeiras.FirstOrDefault(c => c.Id == idCopeira);

            var entregasCopeira = _entregas
                    // CORREÇÃO: Filtra pelo ID e se tem Data Fim (HasValue)
                    .Where(e => e.IdCopeira == idCopeira && e.HoraFim.HasValue)
                    .ToList();

            var viewModel = new RelatorioViewModel
            {
                ChaveTipoRelatorio = "TempoMedioCopeira",
                TipoRelatorio = $"Tempo Médio de Entrega - Copeira: {copeira?.Nome ?? "Desconhecida"}",
                Entregas = entregasCopeira,
                DataInicio = entregasCopeira.Any() ? entregasCopeira.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = entregasCopeira.Any()
                    ? entregasCopeira.Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            if (entregasCopeira.Any())
            {
                // Média Geral
                var tempoMedioGeral = entregasCopeira
                    .Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);

                viewModel.TemposMedios.Add("Tempo Médio Geral", tempoMedioGeral);

                // Média por Tipo de Dieta
                var porDieta = entregasCopeira
                    .Where(e => e.DetalhesDieta != null)
                    .GroupBy(e => e.DetalhesDieta.NomeDieta)
                    .Select(g => new
                    {
                        Dieta = g.Key,
                        TempoMedio = g.Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes)
                    });

                foreach (var item in porDieta)
                {
                    viewModel.TemposMedios.Add($"Dieta {item.Dieta}", item.TempoMedio);
                }
            }

            return viewModel;
        }

        // Outros métodos de relatório podem vir aqui...

        public RelatorioViewModel GerarRelatorioErrosValidacao()
        {
            // (Lógica simplificada para manter o arquivo funcional se você não usar este método agora)
            return new RelatorioViewModel();
        }

        public RelatorioViewModel GerarRelatorioHistoricoPaciente(Guid idPaciente)
        {
            return new RelatorioViewModel();
        }

    } // Fim da Classe
} // Fim do Namespace