using Api_BarberShop.Model;
using Microsoft.EntityFrameworkCore;

namespace Api_BarberShop.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
    }
}
