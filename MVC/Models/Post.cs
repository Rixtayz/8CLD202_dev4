using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models
{

    // 
    // Pendant notre cours nous allons créer une application qui s'apparente a Facebook/Instagram, l'application va nous permettre d'envoyé des photos, d'ajouter des commentaires et des likes.
    // Les structures suivantes démontres en partie les schema que nous allons utiliser
    //

    public enum Category
    {
        Humour = 0,
        Nouvelle = 1,
        Inconnue = 2,
    }


    // Class non mapper créer pour recevoir le fichier du Form
    //
    //

    [NotMapped]
    public class PostForm : Post
    {
        [Required(ErrorMessage = "Un fichier est requis, il doit être de 20 MB et moins")]
        public required IFormFile FileToUpload { get; set; }
    }

    // Implementation du DTO

    [NotMapped]
    public class PostReadDTO
    { 
        public Guid Id { get; init; }

        public string Title { get; init; }

        public Category Category { get; init; }

        public string User { get; init; }

        public int Like { get; init; }

        public int Dislike { get; init; }

        public DateTime Created { get; init; } 

        public string Url { get; init; }

        public PostReadDTO(Post post)
        { 
            Id = post.Id;
            Title = post.Title;
            Category = post.Category;
            User = post.User;
            Like = post.Like;
            Dislike = post.Dislike;
            Created = post.Created;
            Url = post.Url;
        }
    }

    [NotMapped]
    public class PostCreateDTO
    {

        public string Title { get; init; }

        public Category Category { get; init; }

        public string User { get; init; }

        public IFormFile Image { get; init; }

        public PostCreateDTO(string title, Category category, string user, IFormFile image)
        {
            Title = title;
            Category = category;
            User = user;
            Image = image;
        }
    }

    public class Post
    {
        // Référence pour les Accessor ( Get, Set, Init )
        // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/using-properties
        //

        [Key]
        // Ajouter pour NoSQL
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // Utilisation du Guid pour être mieux aligner avec NoSQL
        public Guid Id { get; init; }

        [Required(ErrorMessage = "SVP entrer un titre que personne ne lira")]
        [MaxLength(128)]
        [Display(Name = "Titre")]
        public required string Title { get; init; }

        [Required(ErrorMessage = "SVP entrer une catégorie")]
        [EnumDataType(typeof(Category))]
        [Display(Name = "Catégorie")]
        public Category Category { get; init; }

        [Required(ErrorMessage = "SVP entrer un nom d'utilisateur")]
        [MaxLength(128)]
        [Display(Name = "Nom de l'utilisateur")]
        public required string User { get; init; }

        [Display(Name = "Like")]
        public int Like { get; private set; } = 0;

        [Display(Name = "Dislike")]
        public int Dislike { get; private set; } = 0;

        [Display(Name = "Date de création")]
        public DateTime Created { get; init; } = DateTime.Now;

        [Display(Name = "Contenue revisé ?")]
        public bool IsApproved { get; private set; } = false;

        public bool IsDeleted { get; private set; } = false;

        // Implementation pour entreposé les images sur un Storage Blog
        public required Guid? BlobImage { get; set; }
        
        public required string Url { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public override string ToString()
        {
            return $"===============" + Environment.NewLine +
                $"Title : {Title}" + Environment.NewLine +
                $"Category : {Category}" + Environment.NewLine +
                $"User : {User}" + Environment.NewLine +
                $"Like : {Like}" + Environment.NewLine +
                $"Dislike : {Dislike}" + Environment.NewLine +
                $"Created : {Created}" + Environment.NewLine +
                $"===============";
        }

        public void IncrementLike()
        {
            Like++;
        }

        public void IncrementDislike()
        {
            Dislike++;
        }

        public void Approve()
        {
            IsApproved = true;
        }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
