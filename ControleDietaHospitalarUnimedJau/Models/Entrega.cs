using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Entrega
    {
        public Guid Id { get; set; }
        [Display(Name = "Hora de Início")]
        public DateTime HoraInicio { get; set; }

        [Display(Name = "Hora Final")]
        public DateTime? HoraFim { get; set; }  // ADICIONAR ? AQUI
        public string Status { get; set; }

        [Display(Name = "Observação")]
        public string Observacao { get; set; }
        public Guid PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public Guid CopeiraId { get; set; }
        public Copeira Copeira { get; set; }
        public Guid? DietaId { get; set; }
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