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
        private Dictionary<int, DateTime> FactUsedDateById { get; set; } = [];
        public List<int> FactsOnCooldown => [.. FactUsedDateById.Keys];

        private static readonly int FactCooldownInMinutes = 30;
        private static readonly double FactCheckIntervalInMS = 5000;

        public AlternativeFactCooldownHandler()
        {

            CoolDownTimer = new Timer()
            {
                AutoReset = true,
                Interval = FactCheckIntervalInMS,
            };

            CoolDownTimer.Elapsed += UpdateAlternativeFacts;
           
        }

        private void UpdateAlternativeFacts(object sender, ElapsedEventArgs e)
        {
            var factIDsToRemove = FactUsedDateById.Where(pFact => pFact.Value.AddMinutes(FactCooldownInMinutes) < DateTime.Now).Select(pFact => pFact.Key).ToList();

            if (factIDsToRemove.Count != 0)
            {
                foreach (var factID in factIDsToRemove)
                {
                    FactUsedDateById.Remove(factID);
                }
            }
        }

        public void Add(AlternativeFact fact)
        {
            if(fact != null && !FactUsedDateById.ContainsKey(fact.AlternativeFactID))
                FactUsedDateById.Add(fact.AlternativeFactID, DateTime.Now);
        }
    }
}
