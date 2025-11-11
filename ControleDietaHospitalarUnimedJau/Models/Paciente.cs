using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes; // Necessário para o BsonIgnoreExtraElements
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    // ----- INÍCIO DA CORREÇÃO -----
    // Este atributo diz ao MongoDB para ignorar campos
    // (como "Entregas") que existem no banco mas não nesta classe.
    [BsonIgnoreExtraElements]
    // ----- FIM DA CORREÇÃO -----
    public class Paciente
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("Nome")]
        [Required(ErrorMessage = "O nome do paciente é obrigatório")]
        [StringLength(255)]
        public string Nome { get; set; }

        [BsonElement("NumQuarto")]
        [Required(ErrorMessage = "O número do quarto é obrigatório")]
        [Display(Name = "Número do Quarto")]
        public int NumQuarto { get; set; }

        [BsonElement("CodPulseira")]
        [Required(ErrorMessage = "O código da pulseira é obrigatório")]
        [StringLength(50)]
        [Display(Name = "Código da Pulseira")]
        public string CodPulseira { get; set; }

        [BsonElement("IdDieta")]
        [BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "A dieta do paciente é obrigatória")]
        [Display(Name = "Dieta")]
        public Guid IdDieta { get; set; }

        // Propriedade de navegação para o $lookup (correta, sem [BsonIgnore])
        public Dieta DetalhesDieta { get; set; }
    }
}