using JustShareBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace JustShareBackend.Database
{
    public class JustShareDbContext : DbContext
    {
        public JustShareDbContext(DbContextOptions<JustShareDbContext> options)
            : base(options)
        {
               
        }

        public DbSet<Post> Posts { get; set; }
    }
}
