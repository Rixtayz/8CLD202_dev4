using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos;
using MVC.Models;
using OpenTelemetry.Trace;

namespace MVC.Data
{
    public class EFRepositoryNoSQL : IRepository
    {
        private ApplicationDbContextNoSQL _context;

        public EFRepositoryNoSQL(ApplicationDbContextNoSQL context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetPostsIndex()
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

        public async Task Add(Object Entity)
        {
            _context.Add(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementPostLike(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            post!.IncrementLike();
            await _context.SaveChangesAsync();
        }

        public async Task IncrementPostDislike(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            post!.IncrementDislike();
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsIndex(Guid id)
        {
            // need to order this

            return await _context.Comments.Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync();
        }

        public async Task AddComments(Comment comment)
        {
            // need to confirm this.

            var post = await _context.Posts.FindAsync(comment.PostId);
            post!.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementCommentLike(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            comment!.IncrementLike();
            await _context.SaveChangesAsync();
        }

        public async Task IncrementCommentDislike(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            comment!.IncrementDislike();
            await _context.SaveChangesAsync();
        }

    }
}
