using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Data
{
    public class AlternativeFactRepository : SqlLiteBaseRepository, IAlternativeFactRepository
    {
        public AlternativeFactRepository() : base()
        {

        }
        public List<AlternativeFact> GetFactList(int factID = -1)
        {
            IEnumerable<AlternativeFact> factList;
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            string sql;
            if (factID == -1)
            {
                sql = "SELECT * FROM AlternativeFact WHERE 1=1 ";
            }
            else
            {
                sql = "SELECT * FROM AlternativeFact WHERE AlternativeFactID=@AlternativeFactID ";
            }

            var parameters = new
            {
                AlternativeFactID = factID,
            };
            factList = con.Query<AlternativeFact>(sql, parameters);
            return factList.AsList();
        }

        public void WriteFact(string description, ulong AuthorID)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }

            var sql = "INSERT OR IGNORE INTO AlternativeFact (Description, AuthorID) VALUES (@Description, @AuthorID)";
            var parameters = new
            {
                Description = description,
                AuthorID
            };
            con.Execute(sql, parameters);
        }
    }
}
