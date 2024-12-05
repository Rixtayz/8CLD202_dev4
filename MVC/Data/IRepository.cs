using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepository
    {
        // Post
        Task<List<Post>> GetPostsIndex();

        abstract Task Add(Object Entity);

        abstract Task IncrementPostLike(int id);

        abstract Task IncrementPostDislike(int id);

        // Comments
        Task<List<Comment>> GetCommentsIndex(int id);

        abstract Task AddComments(Comment comment);

        abstract Task IncrementCommentLike(int id);

        abstract Task IncrementCommentDislike(int id);
    }
 }