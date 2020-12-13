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
                        conversion.isExact
                        );
                }
                if (conversion.second.Split(',').Contains(measurement.Name.ToLower()))
                {
                    return new Measurement(
                        (measurement.Amount / conversion.conversionRate) - conversion.offset,
                        conversion.first.Split(',').FirstOrDefault(),
                        conversion.isExact
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

        private static List<(string first, string second,
            float conversionRate, float offset, bool isExact)> ConversionValues =
            new List<(string, string, float, float, bool)> {

                ("ft,feet", "m,meter,meters", 0.3054f, 0, true),
                ("f,fahrenheit", "c,celsius", 0.5555f, -32, true),
                ("kg,kilo,kilogram,kilos,kilograms", "pounds,lb,pound", 2.2046f, 0, true),
                ("km,kilometer,kilometers", "miles,mile,mi", 0.6213f, 0, true),
                ("$,dollar,dollars,usd", "euro,euros,eur", 0.92f, 0, false),
                ("cad", "$", 0.76f, 0, false),
                ("rsd", "$", 0.0093f, 0, false),
                ("£", "$", 1.30f, 0, false),

            };

    }
}