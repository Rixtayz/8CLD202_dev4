using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositorySQL : IRepository
    {
        private ApplicationDbContextSQL _context;

        public EFRepositorySQL(ApplicationDbContextSQL context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetPostsIndex()
        {
            // Ajout d'un "order by", pour trier les resultats
            // Ajout d'un "take", pour prendre seulement une partie des entré, nous ferons une paginations plus tard.
            // Ajout d'un include pour ajouter a notre collection les commentaires lier a notre Post.

            return await _context.Posts.OrderByDescending(o => o.Created).Take(10).Include(i => i.Comments).ToListAsync();
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
            return await _context.Comments.Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync();
        }

        public async Task AddComments(Comment comment)
        {
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
