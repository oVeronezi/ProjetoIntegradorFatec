using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Dieta
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome da Dieta")]
        public string NomeDieta { get; set; }
        [Display(Name = "Itens Alimentares")]
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