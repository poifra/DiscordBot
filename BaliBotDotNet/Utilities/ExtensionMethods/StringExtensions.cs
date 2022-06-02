using System;
namespace BaliBotDotNet.Utilities.ExtensionMethods
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
    }
}
