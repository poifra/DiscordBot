using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Data
{
    public class AuthorRepository : SqlLiteBaseRepository, IAuthorRepository
    {
        public AuthorRepository() : base()
        {

        }
        public Author GetAuthor(ulong authorID)
        {
            Author author;
            using var con = SqlCon;
            var sql = "SELECT * FROM Author WHERE AuthorID=@AuthorID ";
            var parameters = new
            {
                AuthorID = authorID,
            };
            con.Open();
            author = con.Query<Author>(sql, parameters).FirstOrDefault();
            return author;
        }
    }
}
