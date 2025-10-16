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

        public DbSet<Bandeja> Bandeja { get; set; } = default!;
        public DbSet<Paciente> Paciente { get; set; } = default!;
        public DbSet<Copeira> Copeira { get; set; } = default!;
        public DbSet<Dieta> Dietas { get; set; } = default!;
        public DbSet<Entrega> Entrega { get; set; } = default!;
    }
}
