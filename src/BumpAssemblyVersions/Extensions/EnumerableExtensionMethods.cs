using System.Collections.Generic;

namespace Bav
{
    internal static class EnumerableExtensionMethods
    {
        internal static T AddTo<T>(this T value, IList<T> values)
        {
            values.Add(value);
            return value;
        }
    }
}
