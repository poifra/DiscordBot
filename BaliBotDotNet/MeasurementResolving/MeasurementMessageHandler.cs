namespace BalibotTest.MeasurementResolving {
    public static class MeasurementMessageHandler {

        public static string TryConvertMessage(string message) {

            var regexMatches = MeasurementRegexHandler.GetMeasurementsFromMessage(message);

            var resultMessage = "";

            foreach (var regexMatch in regexMatches) {
                var conversionResult = MeasurementConversionHandler.TryConvertFrom(regexMatch);
                if (conversionResult != null) {
                    resultMessage += regexMatch.ToString() + " is " + conversionResult.ToString()+", ";
                }
            }

            if (resultMessage.Length >= 2) {
                return resultMessage.Remove(resultMessage.Length - 2);
            } else {
                return null;
            }

        }
        
    }
}