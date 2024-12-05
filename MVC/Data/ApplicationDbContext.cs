
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using MVC.Models;
using System.Dynamic;
using System.Reflection;


namespace MVC.Data
{
    //dotnet ef migrations add InitialCreate -c ApplicationDbContext
    //dotnet ef migrations add "MigrationName" -c ApplicationDbContext
    //dotnet ef database update -c ContextName

    //dotnet ef database update 0 --context ApplicationDbContext

    public abstract class ApplicationDbContext : DbContext, IRepository
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected abstract override void OnConfiguring(DbContextOptionsBuilder optionsBuilder);

        protected abstract override void OnModelCreating(ModelBuilder modelBuilder);

        public abstract Task<List<Post>> GetIndex();

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
   

    //SQL
    public class ApplicationDbContextSQL : ApplicationDbContext
    {
        public ApplicationDbContextSQL(DbContextOptions options, IConfiguration configuration) : base(options, configuration)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseSqlServer(Configuration.GetConnectionString("LocalSQL")!)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Trace)
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Création de la hiérarchie des tables

            modelBuilder.Entity<Post>()
                .ToTable("Posts")
                .HasMany(m => m.Comments)
                .WithOne(m => m.Post)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .ToTable("Comments");

        }

        public override async Task<List<Post>> GetIndex() 
        {
            // Ajout d'un "order by", pour trier les resultats
            // Ajout d'un "take", pour prendre seulement une partie des entré, nous ferons une paginations plus tard.
            // Ajout d'un include pour ajouter a notre collection les commentaires lier a notre Post.
            // return View(await _context.Posts.OrderByDescending(o => o.Created).Take(10).Include(i => i.Comments).ToListAsync());
            return await Posts.OrderByDescending(o => o.Created).Take(10).Include(i => i.Comments).ToListAsync();
        }

    }

    //No SQL
    public class ApplicationDbContextNoSQL : DbContext
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContextNoSQL(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ajustement de la capacité de la BD
            modelBuilder.HasAutoscaleThroughput(1000);

            //Création de la hiérarchie des tables

            modelBuilder.Entity<Post>()
                .ToContainer("Posts")
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.Id)
                .HasKey(x => x.Id);

            modelBuilder.Entity<Comment>()
                .ToContainer("Comments")
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.PostId)
                .HasKey(x => x.Id);
        }

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}
