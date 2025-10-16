using System;
using System.Collections.Generic;
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
        public string NomeDieta { get; set; }
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