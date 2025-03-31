using Microsoft.EntityFrameworkCore;
using MVC.Business;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;
        private EventHubController _eventHub;

        protected EFRepository(TContext context, EventHubController eventHub) 
        { 
            _context = context; 
            _eventHub = eventHub;
        }

        //Post
        public abstract Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize);
        public virtual async Task<int> GetPostsCount() { return await _context.Set<Post>().CountAsync(); }
        public virtual async Task Add(Post post) 
        { 
            //_context.Add(post); 
            //await _context.SaveChangesAsync(); 

            // Envoie de l'événement
            await _eventHub.SendEvent(new Event(post));

        }
        public virtual async Task IncrementPostLike(Guid id) 
        {
            //var post = await _context.Set<Post>().FindAsync(id); 
            //post!.IncrementLike(); 
            //await _context.SaveChangesAsync(); 

            await _eventHub.SendEvent(new Event(ItemType.Post, Models.Action.Like, id));
        }
        public virtual async Task IncrementPostDislike(Guid id) 
        {
            //var post = await _context.Set<Post>().FindAsync(id); 
            //post!.IncrementDislike(); 
            //await _context.SaveChangesAsync(); 

            await _eventHub.SendEvent(new Event(ItemType.Post, Models.Action.Dislike, id));
        }

        //Comments
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) { return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); }
        public virtual async Task AddComments(Comment comment) 
        {
            //var post = await _context.Set<Post>().FindAsync(comment.PostId); 
            //post!.Comments.Add(comment); 
            //await _context.SaveChangesAsync(); 

            await _eventHub.SendEvent(new Event(comment));
        }
        public virtual async Task IncrementCommentLike(Guid id) 
        {
            //var comment = await _context.Set<Comment>().FindAsync(id); 
            //comment!.IncrementLike(); 
            //await _context.SaveChangesAsync(); 

            await _eventHub.SendEvent(new Event(ItemType.Comment, Models.Action.Like, id));
        }
        public virtual async Task IncrementCommentDislike(Guid id) 
        {
            //var comment = await _context.Set<Comment>().FindAsync(id); 
            //comment!.IncrementDislike(); 
            //await _context.SaveChangesAsync(); 

            await _eventHub.SendEvent(new Event(ItemType.Comment, Models.Action.Dislike, id));
        }
    }
}
