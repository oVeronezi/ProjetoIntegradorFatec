using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class Entrega
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Guid Id { get; set; }

        [BsonElement("IdPaciente")]
        [BsonRepresentation(BsonType.String)]
        [Required]
        [Display(Name = "Paciente")]
        public Guid IdPaciente { get; set; }

        [BsonElement("IdDieta")]
        [BsonRepresentation(BsonType.String)]
        [Required]
        [Display(Name = "Dieta")]
        public Guid? IdDieta { get; set; } // Pode ser nulo

        [BsonElement("IdCopeira")]
        [BsonRepresentation(BsonType.String)]
        [Required]
        [Display(Name = "Copeira")]
        public Guid IdCopeira { get; set; }

        [BsonElement("HoraInicio")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [Display(Name = "Início da Entrega")]
        public DateTime HoraInicio { get; set; }

        [BsonElement("HoraFim")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [Display(Name = "Fim da Entrega")]
        public DateTime HoraFim { get; set; }

        [BsonElement("IdBandeja")]
        [BsonRepresentation(BsonType.String)]
        [Display(Name = "Bandeja")]
        public Guid IdBandeja { get; set; }

        [BsonElement("StatusValidacao")]
        [Display(Name = "Status")]
        public string StatusValidacao { get; set; }

        [BsonElement("Observacao")]
        public string Observacao { get; set; }


        // --- PROPRIEDADES DE NAVEGAÇÃO (CORRIGIDAS) ---

        public Paciente DetalhesPaciente { get; set; }

        public Copeira DetalhesCopeira { get; set; }

        public Bandeja DetalhesBandeja { get; set; }

        public Dieta DetalhesDieta { get; set; }
    }
}