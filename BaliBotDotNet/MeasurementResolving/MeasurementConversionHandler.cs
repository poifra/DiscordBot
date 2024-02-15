using System.Collections.Generic;
using System.Linq;

namespace BalibotTest.MeasurementResolving
{
    public static class MeasurementConversionHandler
    {
        public static List<string> AvailableMeasurementNames;

        public static Measurement TryConvertFrom(Measurement measurement)
        {
            foreach (var conversion in ConversionValues)
            {
                if (conversion.first.Split(',').Contains(measurement.Name.ToLower()))
                {
                    return new Measurement(
                        (measurement.Amount + conversion.offset) * conversion.conversionRate,
                        conversion.second.Split(',').FirstOrDefault(),
                        conversion.canBeNegative
                        );
                }
                if (conversion.second.Split(',').Contains(measurement.Name.ToLower()))
                {
                    return new Measurement(
                        (measurement.Amount / conversion.conversionRate) - conversion.offset,
                        conversion.first.Split(',').FirstOrDefault(),
                        conversion.canBeNegative
                    );
                }
            }
            return null;
        }

        public static void GenerateAvailableMeasurementsList()
        {
            AvailableMeasurementNames = new List<string>();
            foreach (var conversionValue in ConversionValues)
            {
                foreach (var name in conversionValue.first.Split(','))
                {
                    AvailableMeasurementNames.Add(name);
                }
                foreach (var name in conversionValue.second.Split(','))
                {
                    AvailableMeasurementNames.Add(name);
                }
            }
        }

        private static readonly List<(string first, string second,
            float conversionRate, float offset, bool canBeNegative)> ConversionValues =
            new()
            {
                ("ft,feet", "m,meter,meters", 0.3054f, 0, false),
                ("inch,inches", "cm", 2.54f, 0, false),
                ("f,fahrenheit", "c,celsius", 0.5555f, -32, true),
                ("kg,kilo,kilogram,kilos,kilograms", "pounds,lb,pound,lbs", 2.2046f, 0, false),
                ("km,kilometer,kilometers", "miles,mile,mi", 0.6213f, 0, false),
            };

    }
}