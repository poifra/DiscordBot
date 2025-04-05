using BaliBotDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IAlternativeFactRepository
    {
        public List<AlternativeFact> GetFactList(int id);
        public void WriteFact(string description, ulong AuthorID);

    }
}
