using System.Collections.Generic;

namespace BaliBotDotNet.Utilities
{
    public class UOMConverter
    {
        private Dictionary<string, UOM> _uomCache = new Dictionary<string, UOM>();
        private bool _isCacheLoaded = false;

        public double ConvertBetween(UOMCode source, UOMCode destination, double value)
        {
            UOM uomSource = GetUOM(source);
            UOM uomDestination = GetUOM(destination);
            double newValue = value;
            if (uomSource != null
                && uomDestination != null
                && uomSource != uomDestination)
            {
                newValue = ConvertFromBase(ConvertToBase(value, uomSource), uomDestination);
            }
            return newValue;
        }
        private double ConvertToBase(double value, UOM uom)
        {
            return (value + uom.Offset) * uom.Ratio;
        }

        private double ConvertFromBase(double value, UOM uom)
        {
            return value / uom.Ratio - uom.Offset;
        }

        private UOM GetUOM(UOMCode code)
        {
            EnsureCacheIsLoaded();
            if (_uomCache.TryGetValue(code.Code, out UOM result))
            {
                return result;
            }
            return null;
        }
        private void EnsureCacheIsLoaded()
        {
            if (!_isCacheLoaded)
            {
                //simulate load from DB but this will be hardcoded don't tell anyone
            }
            _isCacheLoaded = true;
        }

    }
}
