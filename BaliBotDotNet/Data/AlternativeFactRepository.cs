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
    public class AlternativeFactRepository : IAlternativeFactRepository
    {
        private readonly BaliBotDbContext _db;
        public AlternativeFactRepository(BaliBotDbContext dbContext)
        {
            _db = dbContext;
        }
        public List<AlternativeFact> GetFactList(int factID = -1)
        {
            IQueryable<AlternativeFact> rs = _db.AlternativeFacts;
            if (factID != -1)
            {
                rs = rs.Where(x => x.AlternativeFactID == factID);
            }
            return rs.ToList();
        }

        public void WriteFact(string description, ulong AuthorID)
        {
            _db.AlternativeFacts.Add(new AlternativeFact {Description = description, AuthorID = AuthorID});
            _db.SaveChanges();

        }
    }
}
