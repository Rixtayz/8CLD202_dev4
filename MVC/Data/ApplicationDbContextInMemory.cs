using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVC.Models;


namespace MVC.Data
{
    public class ApplicationDbContextInMemory : DbContext
    {
        public required IConfiguration Configuration { get; set; }
        public ApplicationDbContextInMemory(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }

    // Pour l'identity
    public class ApplicationDbContextInMemoryIdentity : IdentityDbContext
    {
        public ApplicationDbContextInMemoryIdentity(DbContextOptions<ApplicationDbContextInMemoryIdentity> options) : base(options)
        {
        }
    }
}
