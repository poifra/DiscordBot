using BaliBotDotNet.Models;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IAuthorRepository
    {
        Author GetAuthor(ulong authorID);
    }
}
