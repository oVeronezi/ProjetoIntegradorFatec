using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Paciente
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "O nome do paciente é obrigatório")]
        [StringLength(255)]
        public string Nome { get; set; }
        [Required(ErrorMessage = "O número do quarto é obrigatório")]
        public int NumQuarto { get; set; }
        [Required(ErrorMessage = "O código da pulseira é obrigatório")]
        [StringLength(50)]
        public string CodPulseira { get; set; } // alterar aqui caso o código seja apenas de números
        //public int? DietaId { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "A dieta do paciente é obrigatória")]
        public Guid IdDieta { get; set; }


        // 2. O CAMPO PARA RECEBER O OBJETO DIETA (após o lookup)
        // O BsonIgnoreIfNull evita que o driver procure este campo no documento salvo.
        [BsonIgnoreIfNull]
        public Dieta DietaVinculada { get; set; } // Nome sugestivo

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
