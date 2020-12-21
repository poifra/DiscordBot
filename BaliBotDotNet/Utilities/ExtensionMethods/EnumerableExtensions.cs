using System.Collections;

namespace BaliBotDotNet.Utilities.ExtensionMethods
{
    public static class EnumerableExtensions
    {
        public static string Join(this IEnumerable lst, char character)
        {
            return string.Join(character, lst);
        }
    }
}
