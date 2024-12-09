using System.ComponentModel.DataAnnotations.Schema;
using MVC.Models;

namespace MVC.Models
{

    // Implementation du DTO
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
