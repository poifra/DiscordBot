using BaliBotDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Utilities.Interfaces
{
    public interface IAlternativeFactCooldownHandler
    {
        List<int> FactsOnCooldown { get; }

        void Add(AlternativeFact fact);
    }
}
