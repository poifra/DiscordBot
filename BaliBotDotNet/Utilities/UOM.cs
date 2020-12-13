namespace BaliBotDotNet.Utilities
{
    class UOM
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public UOM BaseUOM { get; set; }
        public double Ratio { get; set; }
        public int Offset { get; set; }
    }
}
