using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepositoryAPI<TContext> : IRepositoryAPI where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepositoryAPI(TContext context)
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

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostLike(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                {
                    post!.IncrementLike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostDislike(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                {
                    post!.IncrementDislike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<CommentReadDTO>, NotFound, InternalServerError>> GetAPIComment(Guid id)
        {
            try
            {
                var comment = await _context.Set<Comment>().FirstOrDefaultAsync(w => w.Id == id);
                if (comment == null)
                    return TypedResults.NotFound();
                else
                    return TypedResults.Ok(new CommentReadDTO(comment));
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Created<CommentReadDTO>, NoContent, BadRequest, InternalServerError>> CreateAPIComment(Comment comment)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(comment.PostId);
                if (post == null)
                    return TypedResults.NoContent();
                else
                {
                    post!.Comments.Add(comment);
                    await _context.SaveChangesAsync();
                    return TypedResults.Created($"/Comments/{comment.Id}", new CommentReadDTO(comment));
                }
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

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentLike(Guid id)
        {
            try
            {
                var Comment = await _context.Set<Comment>().FindAsync(id);
                if (Comment == null)
                    return TypedResults.NotFound();
                else
                {
                    Comment!.IncrementLike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentDislike(Guid id)
        {
            try
            {
                var Comment = await _context.Set<Comment>().FindAsync(id);
                if (Comment == null)
                    return TypedResults.NotFound();
                else
                {
                    Comment!.IncrementDislike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

    }
}
