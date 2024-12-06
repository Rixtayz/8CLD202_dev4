using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepository(TContext context) 
        { 
            _context = context; 
        }

        public virtual async Task<List<Post>> GetPostsIndex() { return await _context.Set<Post>().OrderByDescending(o => o.Created).Take(10).ToListAsync(); }
        public virtual async Task Add(Object entity) { _context.Add(entity); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementPostLike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementPostDislike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementDislike(); await _context.SaveChangesAsync(); }
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) { return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); }
        public virtual async Task AddComments(Comment comment) { var post = await _context.Set<Post>().FindAsync(comment.PostId); post!.Comments.Add(comment); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentLike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentDislike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementDislike(); await _context.SaveChangesAsync(); }
    }
}
