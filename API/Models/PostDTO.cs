using System.ComponentModel.DataAnnotations.Schema;
using MVC.Models;

namespace MVC.Models
{

    /// <summary>
    /// Implementation de la class DTO pour la lecture des Post
    /// </summary>
    public class PostReadDTO
    {
        /// <summary>
        /// ID du Post
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Titre du Post
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Catégorie du Post
        /// </summary>
        public Category Category { get; init; }

        /// <summary>
        /// Usager qui a soumis le Post
        /// </summary>
        public string User { get; init; }

        /// <summary>
        /// Nombre de Like
        /// </summary>
        public int Like { get; init; }

        /// <summary>
        /// Nombre de Dislike
        /// </summary>
        public int Dislike { get; init; }

        /// <summary>
        /// Date de création
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// URL de l'image du Post
        /// </summary>
        public string Url { get; init; }

        // Constructeur
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
}
