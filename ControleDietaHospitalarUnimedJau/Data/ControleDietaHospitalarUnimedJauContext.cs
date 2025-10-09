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
    }
}
