using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Utilities.Interfaces;
using System;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class AlternativeFactRepository(BaliBotDbContext dbContext, IAlternativeFactCooldownHandler timerHandler) : IAlternativeFactRepository
    {
        private readonly BaliBotDbContext _db = dbContext;
        private readonly IAlternativeFactCooldownHandler _timerHandler = timerHandler;

        public AlternativeFact GetRandomFact()
        {
            var factsToIgnore = _timerHandler.FactsOnCooldown;
            var rng = new Random();
            var possibleFacts = _db.AlternativeFacts.Where(x => !factsToIgnore.Contains(x.AlternativeFactID)).ToList();
            var fact = possibleFacts[rng.Next(possibleFacts.Count)];

            _timerHandler.Add(fact);

            return fact;
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
            if (_timerHandler.FactsOnCooldown.Contains(factID))
            {
                return null;
            }
            var rs = _db.AlternativeFacts.Find(factID);
            return rs;
        }
    }
}
