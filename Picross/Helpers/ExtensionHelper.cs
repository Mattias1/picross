using System.Linq;

namespace Picross.Helpers
{
    public static class ExtensionHelper
    {
        public static bool IsOneOf<T>(this T enumerator, params T[] values) {
            return values.Contains(enumerator);
        }
    }
}
