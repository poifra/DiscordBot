using BaliBotDotNet.Models;
using BaliBotDotNet.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace BaliBotDotNet.Utilities
{
    public class AlternativeFactCooldownHandler : IAlternativeFactCooldownHandler
    {
        private Timer CoolDownTimer { get; set; }
        private Dictionary<int, DateTime> FactsLastUsedAt { get; set; } = new Dictionary<int, DateTime>();

        public List<int> FactsOnCooldown => [.. FactsLastUsedAt.Keys];

        AlternativeFactCooldownHandler()
        {
            const double timerIntervalInMs = 5000;

            CoolDownTimer = new Timer()
            {
                AutoReset = true,
                Interval = timerIntervalInMs,
            };

            CoolDownTimer.Elapsed += UpdateAlternativeFacts;
           
        }

        private void UpdateAlternativeFacts(object sender, ElapsedEventArgs e)
        {
            const int CoolDownInMinutes = 30;

            var factsToRemove = FactsLastUsedAt.Where(pFact => pFact.Value.AddMinutes(CoolDownInMinutes) < DateTime.Now).Select(pFact => pFact.Key).ToList();

            if (factsToRemove.Any())
            {
                foreach (var fact in factsToRemove)
                {
                    FactsLastUsedAt.Remove(fact);
                }
            }
        }

        public void Add(AlternativeFact fact)
        {
            if(fact != null && !FactsLastUsedAt.ContainsKey(fact.AlternativeFactID))
                FactsLastUsedAt.Add(fact.AlternativeFactID, DateTime.Now);
        }
    }
}
