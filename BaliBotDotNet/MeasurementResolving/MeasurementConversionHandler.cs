using System;
using System.Collections.Generic;
using System.Linq;

namespace BaliBotDotNet.MeasurementResolving
{
    public static class MeasurementConversionHandler
    {
        private static readonly Dictionary<string, UnitInfo> UnitDatabase;
        public static IReadOnlyList<string> AvailableMeasurementNames { get; }
        private class UnitInfo(UnitCategory category, Func<float, float> toBase, Func<float, float> fromBase, bool canBeNegative)
        {
            public UnitCategory Category { get; } = category;
            public Func<float, float> ToBase { get; } = toBase;
            public Func<float, float> FromBase { get; } = fromBase;
            public string PairedUnit { get; set; }
            public bool CanBeNegative { get; } = canBeNegative;
        }

        private class UnitCategory(string name, string baseUnit)
        {
            public string Name { get; } = name;
            public string BaseUnit { get; } = baseUnit;
        }

        static MeasurementConversionHandler()
        {
            //base units
            var length = new UnitCategory("Length", "m");
            var area = new UnitCategory("Area", "m²");
            var temperature = new UnitCategory("Temperature", "°c");
            var mass = new UnitCategory("Mass", "kg");
            var speed = new UnitCategory("Speed", "km/h");
            var illuminance = new UnitCategory("Illuminance", "lux");

            var unitDefinitions = new List<(string Aliases, UnitInfo Info, string PairedAliases)>
            {
                (("m,meter,meters", new UnitInfo(length, v => v, v => v, false), "ft,feet")),
                (("ft,feet", new UnitInfo(length, v => v * 0.3048f, v => v / 0.3048f, false), "m,meter,meters")),
                (("km,kilometer,kilometers", new UnitInfo(length, v => v * 1000f, v => v / 1000f, false), "miles,mile,mi")),
                (("miles,mile,mi", new UnitInfo(length, v => v * 1609.34f, v => v / 1609.34f, false), "km,kilometer,kilometers")),
                (("inch,inches", new UnitInfo(length, v => v * 0.0254f, v => v / 0.0254f, false), "cm")),
                (("cm", new UnitInfo(length, v => v * 0.01f, v => v / 0.01f, false), "inch,inches")),
                
                (("°c,c,celsius", new UnitInfo(temperature, v => v, v => v, true), "°f,f,fahrenheit")),
                (("°f,f,fahrenheit", new UnitInfo(temperature, v => (v - 32) * 5 / 9f, v => (v * 9 / 5f) + 32, true), "°c,c,celsius")),

                (("kg,kilo,kilogram,kilos,kilograms", new UnitInfo(mass, v => v, v => v, false), "pounds,lb,pound,lbs")),
                (("pounds,lb,pound,lbs", new UnitInfo(mass, v => v * 0.453592f, v => v / 0.453592f, false), "kg,kilo,kilogram,kilos,kilograms")),

                (("ac,acre,acres", new UnitInfo(area, v => v * 4046.86f, v => v / 4046.86f, false), "m²")),
                (("m²", new UnitInfo(area, v => v, v => v, false), "ac,acre,acres")),

                (("kmh,km/h", new UnitInfo(speed, v => v, v => v, false), "mph")),
                (("mph", new UnitInfo(speed, v => v * 1.60934f, v => v / 1.60934f, false), "kmh,km/h")),

                (("lumen,lumens", new UnitInfo(illuminance, v => v, v => v, false), "foot candle,fc,ft-c,foot-candle")),
                (("foot candle,fc,ft-c,foot-candle", new UnitInfo(illuminance, v => v * 10.764f, v => v / 10.764f, false), "lumen,lumens")),
            };

            UnitDatabase = new Dictionary<string, UnitInfo>();
            var availableNames = new HashSet<string>();

            foreach (var (aliases, info, pairedAliases) in unitDefinitions)
            {
                var primaryPairedName = pairedAliases.Split(',')[0];
                info.PairedUnit = primaryPairedName;
                
                foreach (var alias in aliases.Split(','))
                {
                    UnitDatabase[alias] = info;
                    availableNames.Add(alias);
                }
            }
            AvailableMeasurementNames = availableNames.ToList();
        }

        public static Measurement TryConvertFrom(Measurement measurement)
        {
            if (UnitDatabase.TryGetValue(measurement.Name.ToLower(), out var unitInfo))
            {
                // Convert source value to the base unit of its category
                var valueInBase = unitInfo.ToBase(measurement.Amount);

                // Find the target unit's info
                if (UnitDatabase.TryGetValue(unitInfo.PairedUnit.ToLower(), out var targetUnitInfo))
                {
                    // Convert from base unit to the target unit
                    var finalValue = targetUnitInfo.FromBase(valueInBase);
                    return new Measurement(finalValue, unitInfo.PairedUnit, targetUnitInfo.CanBeNegative);
                }
            }
            return null;
        }
    }
}

