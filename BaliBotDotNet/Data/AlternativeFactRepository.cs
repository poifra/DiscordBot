using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Utilities;
using BaliBotDotNet.Utilities.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class AlternativeFactRepository(BaliBotDbContext dbContext) : IAlternativeFactRepository
    {
        private readonly BaliBotDbContext _db = dbContext;

        public List<AlternativeFact> GetAllFacts()
        {
            IQueryable<AlternativeFact> rs = _db.AlternativeFacts;
            return rs.ToList();
        }

        public List<AlternativeFact> GetAllFacts(List<int> factsToIgnore)
        {
            return _db.AlternativeFacts.Where(pFact => !factsToIgnore.Contains(pFact.AlternativeFactID))
                                       .ToList();
        }

        public void WriteFact(string description, ulong AuthorID)
        {
            _db.AlternativeFacts.Add(new AlternativeFact {Description = description, AuthorID = AuthorID});
            _db.SaveChanges();
        }

        public void DeleteFact(int factId)
        {
            var fact = _db.AlternativeFacts.Find(factId);

            if (fact != null)
            {
                _db.AlternativeFacts.Remove(fact);
            }

            _db.SaveChanges();
        }

        public AlternativeFact GetFact(int factID)
        {
            var rs = _db.AlternativeFacts.Find(factID);
            return rs;
        }
    }
}
