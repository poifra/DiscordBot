using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly BaliBotDbContext _db;

        public AuthorRepository(BaliBotDbContext dbContext)
        {
            _db = dbContext;
        }
        public Author GetAuthor(ulong authorID)
        {
            return _db.Authors.Where(x=>x.AuthorID ==  authorID).FirstOrDefault();
        }
    }
}
