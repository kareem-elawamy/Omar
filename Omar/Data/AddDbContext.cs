using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Omar.Models;

namespace Omar.Data
{
    public class AddDbContext : IdentityDbContext<ApplicationUser>
    {
        public AddDbContext(DbContextOptions<AddDbContext> options)
            : base(options) { }

        public DbSet<Products> Products { get; set; } = null!;
        public DbSet<StockMovements> StockMovements { get; set; } = null!;
        public DbSet<Sales> Sales { get; set; } = null!;
        public DbSet<SaleItems> SaleItems { get; set; } = null!;
        public DbSet<Expenses> Expenses { get; set; }
    }
}
