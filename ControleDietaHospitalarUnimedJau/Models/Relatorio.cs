using ControleDietaHospitalarUnimedJau.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Relatorio
    {
        // 1. Definição das variáveis (Campos)
        private readonly List<Entrega> _entregas;
        private readonly List<Paciente> _pacientes;
        private readonly List<Copeira> _copeiras;

        // 2. Construtor
        public Relatorio(List<Entrega> entregas, List<Paciente> pacientes, List<Copeira> copeiras)
        {
            _entregas = entregas ?? new List<Entrega>();
            _pacientes = pacientes ?? new List<Paciente>();
            _copeiras = copeiras ?? new List<Copeira>();
        }

        // 3. RELATÓRIO: TEMPO MÉDIO (Já estava funcionando)
        public RelatorioViewModel GerarRelatorioTempoMedioDieta(Guid idCopeira)
        {
            var copeira = _copeiras.FirstOrDefault(c => c.Id == idCopeira);

            var entregasCopeira = _entregas
                    .Where(e => e.IdCopeira == idCopeira && e.HoraFim.HasValue)
                    .ToList();

            var viewModel = new RelatorioViewModel
            {
                ChaveTipoRelatorio = "TempoMedioCopeira",
                TipoRelatorio = $"Tempo Médio de Entrega - Copeira: {copeira?.Nome ?? "Desconhecida"}",
                Entregas = entregasCopeira,
                DataInicio = entregasCopeira.Any() ? entregasCopeira.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = entregasCopeira.Any() ? entregasCopeira.Max(e => e.HoraFim.Value) : DateTime.Now
            };

            if (entregasCopeira.Any())
            {
                var tempoMedioGeral = entregasCopeira.Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);
                viewModel.TemposMedios.Add("Tempo Médio Geral", tempoMedioGeral);

                var porDieta = entregasCopeira
                    .Where(e => e.DetalhesDieta != null)
                    .GroupBy(e => e.DetalhesDieta.NomeDieta)
                    .Select(g => new { Dieta = g.Key, TempoMedio = g.Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes) });

                foreach (var item in porDieta)
                {
                    viewModel.TemposMedios.Add($"Dieta {item.Dieta}", item.TempoMedio);
                }
            }
            return viewModel;
        }

        // 4. RELATÓRIO: ERROS DE VALIDAÇÃO (Implementado)
        public RelatorioViewModel GerarRelatorioErrosValidacao()
        {
            var entregasComErro = _entregas
                .Where(e => e.StatusValidacao == "Erro" ||
                           (e.Observacao != null && e.Observacao.ToLower().Contains("erro")))
                .OrderByDescending(e => e.HoraInicio)
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                TipoRelatorio = "Relatório de Erros de Validação",
                Entregas = entregasComErro,
                TotalErros = entregasComErro.Count,
                DataInicio = entregasComErro.Any() ? entregasComErro.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = DateTime.Now
            };

            return viewModel;
        }

        // 5. RELATÓRIO: HISTÓRICO DO PACIENTE (Implementado)
        public RelatorioViewModel GerarRelatorioHistoricoPaciente(Guid idPaciente)
        {
            var paciente = _pacientes.FirstOrDefault(p => p.Id == idPaciente);

            if (paciente == null)
            {
                return new RelatorioViewModel { TipoRelatorio = "Paciente não encontrado" };
            }

            // Filtra entregas deste paciente
            var entregasPaciente = _entregas
                .Where(e => e.IdPaciente == idPaciente)
                .OrderByDescending(e => e.HoraInicio)
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                TipoRelatorio = $"Histórico do Paciente - {paciente.Nome} (Quarto {paciente.NumQuarto})",
                Entregas = entregasPaciente,
                DataInicio = entregasPaciente.Any() ? entregasPaciente.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = entregasPaciente.Any() && entregasPaciente.Any(e => e.HoraFim.HasValue)
                    ? entregasPaciente.Where(e => e.HoraFim.HasValue).Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            // Preenche as estatísticas para os Cards da View
            viewModel.TotalErros = entregasPaciente.Count(e => e.StatusValidacao == "Erro");

            // Usamos o dicionário TemposMedios para passar contagens simples também
            viewModel.TemposMedios.Add("Entregas Concluídas", entregasPaciente.Count(e => e.StatusValidacao == "Concluído"));

            return viewModel;
        }

    }
}