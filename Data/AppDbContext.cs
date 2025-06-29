using Microsoft.EntityFrameworkCore;
using VesselMovementAPI.Models;

namespace VesselMovementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vessel> Vessels { get; set; }
    }
}