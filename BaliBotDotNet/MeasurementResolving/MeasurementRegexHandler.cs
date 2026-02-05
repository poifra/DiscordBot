using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BaliBotDotNet.MeasurementResolving
{
    public static class MeasurementRegexHandler
    {
        private static readonly Regex MeasurementRegex;

        static MeasurementRegexHandler()
        {
            var allUnits = MeasurementConversionHandler.AvailableMeasurementNames
                .Select(Regex.Escape);
            
            var pattern = $@"([-+]?[0-9]*\.?[0-9]+)\s*({string.Join("|", allUnits)})\b";
            
            MeasurementRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public static List<Measurement> GetMeasurementsFromMessage(string message)
        {
            var measurements = new List<Measurement>();
            var matches = MeasurementRegex.Matches(message);

            foreach (Match match in matches)
            {
                // Group 1 is the number, Group 2 is the unit.
                string numberStr = match.Groups[1].Value;
                string unit = match.Groups[2].Value;

                if (float.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                {
                    measurements.Add(new Measurement(number, unit));
                }
            }
            return measurements;
        }
    }
}
