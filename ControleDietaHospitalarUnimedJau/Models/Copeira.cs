using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Copeira
    {
        public int IdCopeira { get; set; }
        [Required(ErrorMessage = "O nome do funcionário é obrigatório")]
        public string Nome { get; set; }
        public ICollection<Entrega> Entregas { get; set; }

        public Copeira()
        {
            Entregas = new List<Entrega>();
        }

        public void RegistrarEntrega(string codPulseira, string codBandeja)
        {
            // Lógica de registro
        }
        public void AssociarAoPaciente(string CodPulseira) { }
    }
}
