using MVC.Models;

namespace MVC.Data
{
    public interface IRepository
    {
        // Post
        Task<List<Post>> GetPostsIndex();

        abstract Task Add(Post post);

        abstract Task IncrementPostLike(Guid id);

        abstract Task IncrementPostDislike(Guid id);

        // Comments
        Task<List<Comment>> GetCommentsIndex(Guid id);

        abstract Task AddComments(Comment comment);

        abstract Task IncrementCommentLike(Guid id);

        abstract Task IncrementCommentDislike(Guid id);
    }
 }