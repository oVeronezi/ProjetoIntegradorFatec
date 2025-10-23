using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Paciente
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "O nome do paciente é obrigatório")]
        [StringLength(255)]
        public string Nome { get; set; }
        [Required(ErrorMessage = "O número do quarto é obrigatório")]
        [Display(Name = "Número do Quarto")]
        public int NumQuarto { get; set; }
        [Required(ErrorMessage = "O código da pulseira é obrigatório")]
        [StringLength(50)]
        [Display(Name = "Código da Pulseira")]
        public string CodPulseira { get; set; } // alterar aqui caso o código seja apenas de números
        //public int? DietaId { get; set; }
        public Dieta DietaId { get; set; }

        public ICollection<Entrega> Entregas { get; set; }
        public Paciente()
        {
            Entregas = new List<Entrega>();
        }
        public bool ValidarDieta(string CodBandeja) //alterar tipo caso seja diferente
        {
            return true;
        }
        public List<Entrega> ObterHistoricoEntregas()
        {
            return new List<Entrega>(Entregas);
        }
    }
}
