using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepository
    {
        //DbContext Context { get; }

        Task<List<Post>> GetIndex();


    }
}
