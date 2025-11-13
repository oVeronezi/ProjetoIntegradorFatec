using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    [BsonIgnoreExtraElements] // Para ignorar campos antigos
    public class Bandeja
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }

        [BsonElement("CodBandeja")]
        [Display(Name = "Código da Bandeja")]
        [Required(ErrorMessage = "O código da bandeja é obrigatório.")]
        public string CodBandeja { get; set; }

        [BsonElement("TipoDieta")]
        [BsonRepresentation(BsonType.String)]
        [Display(Name = "Tipo da Dieta")]
        [Required(ErrorMessage = "É obrigatório selecionar uma dieta.")]
        public Guid TipoDieta { get; set; }

        // ----- INÍCIO DA MUDANÇA (DELETE LÓGICO) -----
        [BsonElement("Ativo")]
        public bool Ativo { get; set; }
        // ----- FIM DA MUDANÇA -----

        // Propriedade de navegação para $lookup
        public Dieta DetalhesDieta { get; set; }

        // Construtor para definir o padrão
        public Bandeja()
        {
            Ativo = true;
        }
    }
}