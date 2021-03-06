﻿namespace BaliBotDotNet.Utilities.UOM
{
    public class UOMCode
    {
        public string Code { get; private set; }
        protected UOMCode(string code)
        {
            Code = code;
        }

        public static readonly UOMCode Feet = new UOMCode("ft");
        public static readonly UOMCode Meter = new UOMCode("m");
    }
}
