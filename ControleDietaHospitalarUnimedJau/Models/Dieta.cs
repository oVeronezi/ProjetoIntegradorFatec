using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Dieta
    {
        [Required(ErrorMessage = "O nome da dieta é obrigatório")]
        [StringLength(100)]
        public string NomeDieta { get; set; }
        public List<string> ItensAlimentares { get; set; }
        public Dieta()
        {
            ItensAlimentares = new List<string>();
        }
        public List<string> ObterItensDieta()
        {
            return this.ItensAlimentares;
        }
    }
}