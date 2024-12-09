using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositoryInMemory : EFRepository<ApplicationDbContextInMemory>
    {
        public EFRepositoryInMemory(ApplicationDbContextInMemory context) : base(context) { }

        // Le même que SQL, ceci pourrait être bouger dans la base class ...
        public override async Task<List<Post>> GetPostsIndex() { return await _context.Posts.OrderByDescending(o => o.Created).Take(10).Include(i => i.Comments).ToListAsync(); }
    }
}
