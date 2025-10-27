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
        public DateTime? HoraFim { get; set; }  
        public string Status { get; set; }

        [Display(Name = "Observação")]
        public string Observacao { get; set; }
        public Guid IdPaciente { get; set; }
        public Paciente Paciente { get; set; }
        public Guid IdCopeira { get; set; }
        public Copeira Copeira { get; set; }
        public Guid? IdDieta { get; set; }
        public Dieta Dieta { get; set; }

        public Guid? IdBandeja { get; set; }

        public string? StatusValidacao { get; set; }

        public TimeSpan? CalcularTempoEntrega() 
        {
            if (HoraFim.HasValue && HoraFim.Value > HoraInicio)
            {
                return HoraFim.Value - HoraInicio;
            }
            return null;
        }
    }
}