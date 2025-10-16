using MongoDbGenericRepository.Attributes;
using AspNetCore.Identity.MongoDbCore.Models;
namespace ControleDietaHospitalarUnimedJau.Models
{
    [CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string NomeCompleto { get; set; }
    }
}
