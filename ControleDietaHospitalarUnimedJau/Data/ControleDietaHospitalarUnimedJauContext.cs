using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleDietaHospitalarUnimedJau.Models;

namespace ControleDietaHospitalarUnimedJau.Data
{
    public class ControleDietaHospitalarUnimedJauContext : DbContext
    {
        public ControleDietaHospitalarUnimedJauContext (DbContextOptions<ControleDietaHospitalarUnimedJauContext> options)
            : base(options)
        {
        }

        public DbSet<ControleDietaHospitalarUnimedJau.Models.Bandeja> Bandeja { get; set; } = default!;
        public DbSet<ControleDietaHospitalarUnimedJau.Models.Paciente> Paciente { get; set; } = default!;
        public DbSet<ControleDietaHospitalarUnimedJau.Models.Copeira> Copeira { get; set; } = default!;
        public DbSet<ControleDietaHospitalarUnimedJau.Models.Dieta> Dieta { get; set; } = default!;
        public DbSet<ControleDietaHospitalarUnimedJau.Models.Entrega> Entrega { get; set; } = default!;
    }
}
