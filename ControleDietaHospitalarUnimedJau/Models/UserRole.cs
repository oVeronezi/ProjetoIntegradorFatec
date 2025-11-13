using System.ComponentModel.DataAnnotations;

namespace ControleDietaHospitalarUnimedJau.Models
{
    public class UserRole
    {
        [Required]
        public string? RoleName { get; set; }
    }
}
