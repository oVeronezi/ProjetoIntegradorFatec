using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    // Adiciona o [BsonIgnoreExtraElements] para segurança
    [BsonIgnoreExtraElements]
    public class Copeira
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }

        [BsonElement("Nome")] // Mapeamento explícito
        [Required(ErrorMessage = "O nome do funcionário é obrigatório")]
        public string Nome { get; set; }

        // ----- INÍCIO DA MUDANÇA (DELETE LÓGICO) -----
        [BsonElement("Ativo")]
        public bool Ativo { get; set; }
        // ----- FIM DA MUDANÇA -----


        // --- O CONSTRUTOR FOI ATUALIZADO ---
        public Copeira()
        {
            // Define o valor padrão para novas copeiras
            Ativo = true;
        }
    }
}