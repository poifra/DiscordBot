using System.Collections.Generic;

namespace BaliBotDotNet.Utilities.ExtensionMethods
{
    public static class EnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> lst, char character) => string.Join(character, lst);
    }
}
