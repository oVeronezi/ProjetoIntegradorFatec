using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Bandeja
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "O código da bandeja é obrigatório")]
        public string CodBandeja { get; set; } // mudar o tipo conforme necessário
        [Required(ErrorMessage = "O Tipo de Dieta é obrigatório")]
        public string TipoDieta { get; set; }
        public void AssociarAoPaciente(string CodPulseira) { }
    }
}
