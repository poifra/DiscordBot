namespace BalibotTest.MeasurementResolving {
    public class Measurement {
        public float Amount;
        public string Name;
        public bool IsExact;

        public Measurement(float amount, string name, bool isExact=true) {
            Amount = amount;
            Name = name;
            IsExact = isExact;
        }

        public override string ToString() {
            return (IsExact ? "" : "about ") + Amount + Name;
        }
    }
}