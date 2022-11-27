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
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sql = "SELECT * FROM Author WHERE AuthorID=@AuthorID ";
            var parameters = new
            {
                AuthorID = authorID,
            };
            author = con.Query<Author>(sql, parameters).FirstOrDefault();
            return author;
        }
    }
}
