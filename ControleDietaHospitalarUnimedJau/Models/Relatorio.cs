using ControleDietaHospitalarUnimedJau.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControleDietaHospitalarUnimedJau.Models
{
    // Classe Relatorio - Serviço para geração de relatórios
    public class Relatorio
    {
        // Dependências para acesso aos dados
        private readonly List<Entrega> _entregas;
        private readonly List<Paciente> _pacientes;
        private readonly List<Copeira> _copeiras;

        // Construtor que recebe as coleções de dados
        public Relatorio(List<Entrega> entregas, List<Paciente> pacientes, List<Copeira> copeiras)
        {
            _entregas = entregas ?? new List<Entrega>();
            _pacientes = pacientes ?? new List<Paciente>();
            _copeiras = copeiras ?? new List<Copeira>();
        }

        // Gera relatório de tempo médio de entrega por copeira
        public RelatorioViewModel GerarRelatorioTempoMedioDieta(Guid idCopeira)
        {
            var copeira = _copeiras.FirstOrDefault(c => c.Id == idCopeira);

            var entregasCopeira = _entregas
                .Where(e => e.IdCopeira == idCopeira && e.StatusValidacao == "Concluída")
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                TipoRelatorio = $"Tempo Médio de Entrega - Copeira: {copeira?.Nome ?? "Desconhecida"}",
                Entregas = entregasCopeira,
                DataInicio = entregasCopeira.Any() ? entregasCopeira.Min(e => e.HoraInicio) : DateTime.Now,

                // ----- CORREÇÃO DE "DATETIME?" (Linha 48) -----
                // Trocado de "e.HoraFim > DateTime.MinValue" para "e.HoraFim.HasValue"
                // Trocado de "e.HoraFim" para "e.HoraFim.Value"
                DataFim = entregasCopeira.Any() && entregasCopeira.Any(e => e.HoraFim.HasValue)
                    ? entregasCopeira.Where(e => e.HoraFim.HasValue).Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            if (entregasCopeira.Any())
            {
                // ----- CORREÇÃO DE "DATETIME?" (Linha 54) -----
                // Trocado para .HasValue
                var entregasComTempo = entregasCopeira.Where(e => e.HoraFim.HasValue).ToList();

                if (entregasComTempo.Any())
                {
                    var tempoMedioGeral = entregasComTempo
                        // ----- CORREÇÃO DE "TIMESPAN?" (Linha 60) -----
                        // Trocado para .HoraFim.Value (corrige o CS1061)
                        .Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);

                    viewModel.TemposMedios.Add("Tempo Médio Geral", tempoMedioGeral);

                    var porDieta = entregasComTempo
                        .Where(e => e.DetalhesDieta != null)
                        .GroupBy(e => e.DetalhesDieta.NomeDieta)
                        .Select(g => new
                        {
                            Dieta = g.Key,
                            // ----- CORREÇÃO DE "TIMESPAN?" (Linha 70) -----
                            // Trocado para .HoraFim.Value (corrige o CS1061)
                            TempoMedio = g.Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes)
                        });

                    foreach (var item in porDieta)
                    {
                        viewModel.TemposMedios.Add($"Dieta {item.Dieta}", item.TempoMedio);
                    }
                }
            }

            return viewModel;
        }

        // Gera relatório de erros de validação
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

            if (entregasComErro.Any())
            {
                var errosPorCopeira = entregasComErro
                    .GroupBy(e => e.IdCopeira)
                    .ToDictionary(
                        g => $"Copeira ID {g.Key}",
                        g => (double)g.Count()
                    );

                foreach (var item in errosPorCopeira)
                {
                    viewModel.TemposMedios.Add(item.Key, item.Value);
                }
            }

            return viewModel;
        }

        // Gera relatório de histórico de um paciente
        public RelatorioViewModel GerarRelatorioHistoricoPaciente(Guid idPaciente)
        {
            var paciente = _pacientes.FirstOrDefault(p => p.Id == idPaciente);

            if (paciente == null)
            {
                return new RelatorioViewModel { /*...*/ };
            }

            var entregasPaciente = _entregas
                .Where(e => e.IdPaciente == idPaciente)
                .OrderByDescending(e => e.HoraInicio)
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                TipoRelatorio = $"Histórico do Paciente - {paciente.Nome} (Quarto {paciente.NumQuarto})",
                Entregas = entregasPaciente,
                DataInicio = entregasPaciente.Any() ? entregasPaciente.Min(e => e.HoraInicio) : DateTime.Now,

                // ----- CORREÇÃO DE "DATETIME?" (Linha 146) -----
                // Trocado para .HasValue e .Value
                DataFim = entregasPaciente.Any() && entregasPaciente.Any(e => e.HoraFim.HasValue)
                    ? entregasPaciente.Where(e => e.HoraFim.HasValue).Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            if (entregasPaciente.Any())
            {
                var totalEntregas = entregasPaciente.Count;
                var entregasConcluidas = entregasPaciente.Count(e => e.StatusValidacao == "Concluída");
                var entregasComErro = entregasPaciente.Count(e => e.StatusValidacao == "Erro");

                viewModel.TemposMedios.Add("Total de Entregas", totalEntregas);
                viewModel.TemposMedios.Add("Entregas Concluídas", entregasConcluidas);
                viewModel.TemposMedios.Add("Entregas com Erro", entregasComErro);
                viewModel.TotalErros = entregasComErro;

                var entregasComTempo = entregasPaciente
                    // ----- CORREÇÃO DE "DATETIME?" (Linha 167) -----
                    .Where(e => e.HoraFim.HasValue && e.StatusValidacao == "Concluída")
                    .ToList();

                if (entregasComTempo.Any())
                {
                    var tempoMedio = entregasComTempo
                        // ----- CORREÇÃO DE "TIMESPAN?" (Linha 173) -----
                        // Trocado para .HoraFim.Value (corrige o CS1061)
                        .Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);

                    viewModel.TemposMedios.Add("Tempo Médio de Entrega (min)", tempoMedio);
                }
            }

            return viewModel;
        }
    }
}