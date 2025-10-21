using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MongoDB.Driver;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class ContextMongodb
    {
        public static string? ConnectionString { get; set; }
        public static string? Database { get; set; }
        public static bool IsSSL { get; set; }
        private IMongoDatabase _database { get; }


        public ContextMongodb()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoCliente = new MongoClient(settings);
                _database = mongoCliente.GetDatabase(Database);

            }
            catch (Exception)
            {
                throw new Exception("Não foi possível conectar Mongodb");
            }

        }//fim do construtor
        public IMongoCollection<Copeira> Copeiras
        {
            get
            {
                return _database.GetCollection<Copeira>("Copeiras");
            }

        }
        public IMongoCollection<Dieta> Dietas 
        { 
            get
            {
                return _database.GetCollection<Dieta>("Dietas");
            }
            
        }
        public IMongoCollection<Entrega> Entregas
        {
            get
            {
                return _database.GetCollection<Entrega>("Entregas");
            }
        }
        public IMongoCollection<Paciente> Pacientes
        {
            get
            {
                return _database.GetCollection<Paciente>("Pacientes");
            }
        }
        
    }//fim da classe
}
