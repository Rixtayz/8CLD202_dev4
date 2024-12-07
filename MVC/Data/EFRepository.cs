using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Business;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepository(TContext context) 
        { 
            _context = context; 
        }

        //API
        // Avec l'implementation du DTO
        public virtual async Task<Results<Ok<List<PostReadDTO>>, InternalServerError>> GetAPIPostsIndex() 
        {
            try
            {
                // Converstion dans le DTO
                Post[] posts = await _context.Set<Post>().ToArrayAsync();
                List<PostReadDTO> postsDTO = posts.Select(x => new PostReadDTO(x)).ToList();

                return TypedResults.Ok(postsDTO);
            }
            catch 
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<PostReadDTO>, NotFound, InternalServerError>> GetAPIPost(Guid id) 
        {
            try
            {
                var post = await _context.Set<Post>().FirstOrDefaultAsync(w => w.Id == id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                    return TypedResults.Ok(new PostReadDTO(post));
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post)
        {
            try
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return TypedResults.Created($"/Posts/{post.Id}", new PostReadDTO(post));

            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                return TypedResults.BadRequest();
            }
            catch (Exception)
            { 
                return TypedResults.InternalServerError();
            }
        }

        //Post
        public abstract Task<List<Post>> GetPostsIndex();
        public virtual async Task Add(Post post) { _context.Add(post); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementPostLike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementPostDislike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementDislike(); await _context.SaveChangesAsync(); }

        //Comments
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) { return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); }
        public virtual async Task AddComments(Comment comment) { var post = await _context.Set<Post>().FindAsync(comment.PostId); post!.Comments.Add(comment); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentLike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentDislike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementDislike(); await _context.SaveChangesAsync(); }
    }
}
