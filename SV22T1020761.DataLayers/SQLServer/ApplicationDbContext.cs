using Microsoft.EntityFrameworkCore;
using SV22T1020761.Models.Catalog;

namespace SV22T1020761.DataLayers.SQLServer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}