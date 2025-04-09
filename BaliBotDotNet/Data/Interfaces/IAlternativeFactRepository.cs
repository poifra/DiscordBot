using BaliBotDotNet.Models;
using System.Collections.Generic;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IAlternativeFactRepository
    {
        public AlternativeFact GetRandomFact();
        public AlternativeFact GetFact(int factID);
        public void WriteFact(string description, ulong AuthorID);
        public void DeleteFact(int factId);
    }
}
