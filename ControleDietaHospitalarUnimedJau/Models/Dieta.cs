using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System; // 👈 Adicionado para o tipo Guid
using System.Collections.Generic; // 👈 Adicionado para o tipo List<string>

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Dieta
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }

        // Mapeamento corrigido: Liga "nomeDieta" (banco) a NomeDieta (C#)
        [BsonElement("NomeDieta")]
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome da Dieta")]
        public string NomeDieta { get; set; }

        // Mapeamento adicionado para garantir que o array "itensAlimentares" seja lido
        [BsonElement("ItensAlimentares")]
        [Display(Name = "Itens Alimentares")]
        public List<string> ItensAlimentares { get; set; }
        [BsonElement("Ativo")]
        public bool Ativo { get; set; }

        // Construtor para inicializar a lista de itens, evitando NullReferenceException
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