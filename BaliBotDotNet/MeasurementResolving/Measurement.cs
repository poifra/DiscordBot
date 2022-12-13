using System;
using System.Linq;

namespace BalibotTest.MeasurementResolving
{
    public class Measurement
    {
        public float Amount;
        public string Name;
        public bool IsExact;

        public Measurement(float amount, string name, bool isExact = true)
        {
            Amount = amount;
            Name = name;
            IsExact = isExact;
        }

        public override string ToString()
        {
            var conversion = (IsExact ? "" : "about ") + Amount.ToString("0.##") + " " + Name;
            //var random = new Random();
            //var final = conversion.Select(x => random.Next() % 2 == 0 ?
            //(char.IsUpper(x) ? x.ToString().ToLower().First() : 
            //x.ToString().ToUpper().First()) : x);

            //  return new string(final.ToArray());
            return conversion;
        }
    }
}