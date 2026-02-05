using System.Linq;

namespace BaliBotDotNet.MeasurementResolving
{
    public static class MeasurementMessageHandler
    {
        private const float MinAmountToConvert = -9999999; //added to prevent balibot from triggering when people are exaggerating
        private const float MaxAmountToConvert = 9999999;

        public static string TryConvertMessage(string message)
        {
            var regexMatches = MeasurementRegexHandler.GetMeasurementsFromMessage(message);

            var convertedParts = regexMatches
                .Select(match => (match, result: MeasurementConversionHandler.TryConvertFrom(match)))
                .Where(t => t.result != null &&
                             t.result.Amount != 0 &&
                             t.result.Amount is > MinAmountToConvert and < MaxAmountToConvert &&
                             (t.result.CanBeNegative || t.result.Amount > 0))
                .Select(t => $"{t.match} is {t.result}");

            var resultMessage = string.Join(", ", convertedParts);

            return string.IsNullOrEmpty(resultMessage) ? null : resultMessage;
        }
    }
}