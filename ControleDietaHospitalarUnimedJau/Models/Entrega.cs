using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Entrega
    {
        public int IdEntrega { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFim {  get; set; }
        public string Status { get; set; }
        public string Observacao { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public int CopeiraId { get; set; }
        public Copeira Copeira { get; set; }
        public int? DietaId { get; set; }
        public Dieta Dieta { get; set; }

        public TimeSpan CalcularTempoEntrega()
        {
            if(HoraFim > HoraInicio)
            {
                return HoraFim - HoraInicio;
            }
            return TimeSpan.Zero;
        }
    }
}
