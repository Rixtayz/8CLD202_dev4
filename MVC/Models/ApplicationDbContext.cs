
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Identity.Client;

namespace MVC.Models
{
    //dotnet ef migrations add InitialCreate -c jokecontext
    //dotnet ef migrations add "MigrationName" -c ContextName
    //dotnet ef database update -c ContextName


    //SQL
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            base.OnConfiguring(optionsBuilder);
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

            //Ajout de données de seed sur notre BD

            modelBuilder.Entity<Post>().HasData(
                    new Post { Id = -2, Title = "Meme2", Category = Category.Humour, User = "Guillaume Routhier", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme2.png")},
                    new Post { Id = -3, Title = "Meme3", Category = Category.Humour, User = "Guillaume R.", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme3.png") },
                    new Post { Id = -4, Title = "Meme4", Category = Category.Humour, User = "Guillaume R", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme4.jpg") }
                );
        }
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;

    }

    //No SQL
    public class ApplicationDbContextNoSQL : DbContext
    {
        public ApplicationDbContextNoSQL(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ajustement de la capacité de la BD
            modelBuilder.HasAutoscaleThroughput(1000);

            //Création de la hiérarchie des tables

            modelBuilder.Entity<Post>()
                .ToContainer("Post")
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.Created)
                .HasKey(x => x.Id);

            modelBuilder.Entity<Comment>()
                .ToContainer("Comments")
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.PostId)
                .HasKey(x => x.Id);

            //Ajout de données de seed sur notre BD
            modelBuilder.Entity<Post>().HasData(
                    new Post { Id = -2, Title = "Meme2", Category = Category.Humour, User = "Guillaume Routhier", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme2.png") },
                    new Post { Id = -3, Title = "Meme3", Category = Category.Humour, User = "Guillaume R.", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme3.png") },
                    new Post { Id = -4, Title = "Meme4", Category = Category.Humour, User = "Guillaume R", Image = File.ReadAllBytes(AppContext.BaseDirectory + "Meme4.jpg") }
                );

        }

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}
