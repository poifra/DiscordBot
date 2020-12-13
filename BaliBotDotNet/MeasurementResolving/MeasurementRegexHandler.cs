using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BalibotTest.MeasurementResolving
{
    public static class MeasurementRegexHandler
    {
        public static List<Measurement> GetMeasurementsFromMessage(string message)
        {
            var Measurements = new List<Measurement>();

            foreach (var measurementName in
                MeasurementConversionHandler.AvailableMeasurementNames)
            {
                var regex = new Regex(@"([-+]?[0-9]*\.?[0-9]+)\s*(" +
                                      measurementName + @")([\s\t\n]+|$)",
                                        RegexOptions.IgnoreCase);
                var matches = regex.Matches(message);
                foreach (Match match in matches)
                {
                    var unit = match?.Groups[2].ToString();
                    var wholeMatch = match?.Groups[0].ToString().Trim();

                    if (unit == null
                        || match == null
                        || !double.TryParse(wholeMatch.Substring
                            (0, wholeMatch.Length - unit.Length),
                            NumberStyles.Float, CultureInfo.InvariantCulture,
                            out double number))
                    {
                        continue;
                    }

                    Measurements.Add(new Measurement((float)number, unit));
                }
            }
            return Measurements;
        }

    }
}