namespace BalibotTest.MeasurementResolving
{
    public static class MeasurementMessageHandler
    {
	    private const float MinAmountToConvert = -9999999; //added to prevent balibot from triggering when people are exaggerating
	    private const float MaxAmountToConvert = 9999999;

	    public static string TryConvertMessage(string message)
        {

            var regexMatches = MeasurementRegexHandler.GetMeasurementsFromMessage(message);

            var resultMessage = "";

            foreach (var regexMatch in regexMatches)
            {
                var conversionResult = MeasurementConversionHandler.TryConvertFrom(regexMatch);
                if (conversionResult != null && conversionResult.Amount != 0 
                                             && conversionResult.Amount is > MinAmountToConvert and < MaxAmountToConvert 
                                             && (conversionResult.canBeNegative || conversionResult.Amount > 0))
                {
                    resultMessage += regexMatch.ToString() + " is " + conversionResult.ToString() + ", ";
                }
            }

            if (resultMessage.Length >= 2)
            {
                return resultMessage.Remove(resultMessage.Length - 2);
            }
            else
            {
                return null;
            }

        }

    }
}