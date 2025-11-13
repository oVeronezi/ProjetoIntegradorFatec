using ControleDietaHospitalarUnimedJau.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControleDietaHospitalarUnimedJau.Models
{
    // Classe Relatorio - Serviço para geração de relatórios
    public class Relatorio
    {
        // ... (Seu construtor e propriedades privadas aqui, não mudam) ...
        private readonly List<Entrega> _entregas;
        private readonly List<Paciente> _pacientes;
        private readonly List<Copeira> _copeiras;

        public Relatorio(List<Entrega> entregas, List<Paciente> pacientes, List<Copeira> copeiras)
        {
            _entregas = entregas ?? new List<Entrega>();
            _pacientes = pacientes ?? new List<Paciente>();
            _copeiras = copeiras ?? new List<Copeira>();
        }


        // ===============================================
        // MÉTODO 1: TEMPO MÉDIO (COM A CHAVE)
        // ===============================================
        public RelatorioViewModel GerarRelatorioTempoMedioDieta(Guid idCopeira)
        {
            var copeira = _copeiras.FirstOrDefault(c => c.Id == idCopeira);

            var entregasCopeira = _entregas
                    .Where(e => e.IdCopeira == idCopeira &&
                                (e.StatusValidacao == "Concluída" || e.StatusValidacao == "Correta"))
                    .ToList();

            var viewModel = new RelatorioViewModel
            {
                // ✅ ADIÇÃO DA CHAVE FIXA
                ChaveTipoRelatorio = "TempoMedioCopeira",

                // O título de exibição (dinâmico)
                TipoRelatorio = $"Tempo Médio de Entrega - Copeira: {copeira?.Nome ?? "Desconhecida"}",

                Entregas = entregasCopeira,
                DataInicio = entregasCopeira.Any() ? entregasCopeira.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = entregasCopeira.Any() && entregasCopeira.Any(e => e.HoraFim.HasValue)
                    ? entregasCopeira.Where(e => e.HoraFim.HasValue).Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            // ... (O resto da sua lógica de cálculo de tempo médio continua aqui) ...
            if (entregasCopeira.Any())
            {
                var entregasComTempo = entregasCopeira.Where(e => e.HoraFim.HasValue).ToList();
                if (entregasComTempo.Any())
                {
                    var tempoMedioGeral = entregasComTempo
                        .Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);
                    viewModel.TemposMedios.Add("Tempo Médio Geral", tempoMedioGeral);

                    var porDieta = entregasComTempo
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
            }
            return viewModel;
        }

        // ===============================================
        // MÉTODO 2: ERROS (COM A CHAVE)
        // ===============================================
        public RelatorioViewModel GerarRelatorioErrosValidacao()
        {
            var entregasComErro = _entregas
                .Where(e => e.StatusValidacao == "Erro" ||
                            (e.Observacao != null && e.Observacao.ToLower().Contains("erro")))
                .OrderByDescending(e => e.HoraInicio)
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                // ✅ ADIÇÃO DA CHAVE FIXA
                ChaveTipoRelatorio = "ErrosValidacao",

                TipoRelatorio = "Relatório de Erros de Validação",
                Entregas = entregasComErro,
                TotalErros = entregasComErro.Count,
                DataInicio = entregasComErro.Any() ? entregasComErro.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = DateTime.Now
            };

            // ... (O resto da sua lógica de erros continua aqui) ...
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

        // ===============================================
        // MÉTODO 3: PACIENTE (COM A CHAVE)
        // ===============================================
        public RelatorioViewModel GerarRelatorioHistoricoPaciente(Guid idPaciente)
        {
            var paciente = _pacientes.FirstOrDefault(p => p.Id == idPaciente);

            if (paciente == null)
            {
                return new RelatorioViewModel
                {
                    // ✅ ADIÇÃO DA CHAVE FIXA (para o caso de erro)
                    ChaveTipoRelatorio = "NaoEncontrado",
                    TipoRelatorio = "Erro: Paciente não encontrado"
                };
            }

            var entregasPaciente = _entregas
                .Where(e => e.IdPaciente == idPaciente)
                .OrderByDescending(e => e.HoraInicio)
                .ToList();

            var viewModel = new RelatorioViewModel
            {
                // ✅ ADIÇÃO DA CHAVE FIXA
                ChaveTipoRelatorio = "HistoricoPaciente",

                TipoRelatorio = $"Histórico do Paciente - {paciente.Nome} (Quarto {paciente.NumQuarto})",
                Entregas = entregasPaciente,
                DataInicio = entregasPaciente.Any() ? entregasPaciente.Min(e => e.HoraInicio) : DateTime.Now,
                DataFim = entregasPaciente.Any() && entregasPaciente.Any(e => e.HoraFim.HasValue)
                    ? entregasPaciente.Where(e => e.HoraFim.HasValue).Max(e => e.HoraFim.Value)
                    : DateTime.Now
            };

            // ... (O resto da sua lógica de histórico continua aqui) ...
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
                    .Where(e => e.HoraFim.HasValue && e.StatusValidacao == "Concluída")
                    .ToList();

                if (entregasComTempo.Any())
                {
                    var tempoMedio = entregasComTempo
                        .Average(e => (e.HoraFim.Value - e.HoraInicio).TotalMinutes);
                    viewModel.TemposMedios.Add("Tempo Médio de Entrega (min)", tempoMedio);
                }
            }
            return viewModel;
        }
    }
}