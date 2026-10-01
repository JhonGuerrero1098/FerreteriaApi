using FerreteriaAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace FerreteriaAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }


    }
}
