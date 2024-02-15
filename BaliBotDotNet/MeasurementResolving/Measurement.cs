using System;
using System.Linq;

namespace BalibotTest.MeasurementResolving
{
    public class Measurement
    {
        public float Amount;
        public string Name;
        public bool CanBeNegative;

        public Measurement(float amount, string name, bool canBeNegative = true)
        {
            Amount = amount;
            Name = name;
            canBeNegative = canBeNegative;
        }

        public override string ToString()
        {
	        var conversion = Amount.ToString("0.##") + " " + Name;
            //var random = new Random();
            //var final = conversion.Select(x => random.Next() % 2 == 0 ?
            //(char.IsUpper(x) ? x.ToString().ToLower().First() : 
            //x.ToString().ToUpper().First()) : x);

            //  return new string(final.ToArray());
            return conversion;
        }
    }
}