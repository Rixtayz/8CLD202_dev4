
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


    //SQL
    public class ApplicationDbContextSQL : DbContext
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContextSQL(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
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

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }

    //No SQL
    public class ApplicationDbContextNoSQL : DbContext
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContextNoSQL(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCosmos(
                // Change for dynamic
                    connectionString: "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    databaseName: "ApplicationDB",
                    cosmosOptionsAction: options =>
                    {
                        options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
                        options.MaxRequestsPerTcpConnection(16);
                        options.MaxTcpConnectionsPerEndpoint(32);
                    });
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
