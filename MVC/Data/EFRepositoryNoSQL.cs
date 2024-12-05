using Microsoft.EntityFrameworkCore;
using MVC.Models;

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
            // Need to order this.

            return await _context.Posts.ToListAsync();
        }

        public async Task Add(Object Entity)
        {
            _context.Add(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementPostLike(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            post!.IncrementLike();
            await _context.SaveChangesAsync();
        }

        public async Task IncrementPostDislike(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            post!.IncrementDislike();
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsIndex(int id)
        {
            // need to order this

            return await _context.Comments.ToListAsync();
        }

        public async Task AddComments(Comment comment)
        {
            // need to confirm this.

            var post = _context.Posts.Where(w => w.Id == comment.PostId).FirstOrDefault();
            post!.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementCommentLike(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            comment!.IncrementLike();
            await _context.SaveChangesAsync();
        }

        public async Task IncrementCommentDislike(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            comment!.IncrementDislike();
            await _context.SaveChangesAsync();
        }

    }
}
