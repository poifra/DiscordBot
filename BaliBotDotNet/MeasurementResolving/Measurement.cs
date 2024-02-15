namespace BalibotTest.MeasurementResolving
{
    public class Measurement
    {
        public float Amount;
        public string Name;
        public bool canBeNegative;

        public Measurement(float amount, string name, bool canBeNegative = true)
        {
            Amount = amount;
            Name = name;
            canBeNegative = canBeNegative;
        }

        public override string ToString()
        {
            return (canBeNegative ? "" : "about ") + Amount.ToString("0.##") + " " + Name;
        }
    }
}