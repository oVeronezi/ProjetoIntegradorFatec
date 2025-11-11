using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System; // <-- Adicionado para o tipo Guid
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Bandeja
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }

        // Mapeamento explícito para o banco de dados
        [BsonElement("CodBandeja")]
        [Display(Name = "Código da Bandeja")]
        [Required(ErrorMessage = "O código da bandeja é obrigatório.")]
        public string CodBandeja { get; set; }

        // ----- CORREÇÃO DE TIPO -----
        // O TipoDieta é um ID de referência (Guid), 
        // mas guardado como String no banco.
        [BsonElement("TipoDieta")]
        [BsonRepresentation(BsonType.String)]
        [Display(Name = "Tipo da Dieta")]
        [Required(ErrorMessage = "É obrigatório selecionar uma dieta.")]
        public Guid TipoDieta { get; set; } // <-- Alterado de string para Guid
        public Dieta DetalhesDieta { get; set; }
    }
}