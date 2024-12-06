using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositoryNoSQL : EFRepository<ApplicationDbContextNoSQL>
    {
        public EFRepositoryNoSQL(ApplicationDbContextNoSQL context) : base(context) { }

        public override async Task<List<Post>> GetPostsIndex()
        {
            // En NoSQL nous ne pouvons pas faire de "include" nous devons donc faire 2 query et les merger.
            List<Post> posts = await _context.Posts.OrderByDescending(o => o.Created).Take(10).ToListAsync();

            // Nous extractons ensuite la list de Guid des posts
            List<Guid> postGuid = posts.Select(p => p.Id).ToList();

            // Nous extractons ensuite les comments relier au posts.
            List<Comment> comments = await _context.Comments.Where(c => postGuid.Contains(c.PostId)).ToListAsync();

            // Agregation du lots
            foreach (var post in posts)
            { 
                post.Comments = comments.Where(w => w.PostId == post.Id).ToList();
            }

            return posts;
        }
    }
}
