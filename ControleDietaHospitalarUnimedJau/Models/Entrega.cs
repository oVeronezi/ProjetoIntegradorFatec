using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Entrega
    {
        public int IdEntrega { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime? HoraFim { get; set; }  // ADICIONAR ? AQUI
        public string Status { get; set; }
        public string Observacao { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public int CopeiraId { get; set; }
        public Copeira Copeira { get; set; }
        public int? DietaId { get; set; }
        public Dieta Dieta { get; set; }

        public TimeSpan? CalcularTempoEntrega()  // TAMBÉM TORNAR nullable
        {
            if (HoraFim.HasValue && HoraFim.Value > HoraInicio)
            {
                return HoraFim.Value - HoraInicio;
            }
            return null;
        }
    }
}