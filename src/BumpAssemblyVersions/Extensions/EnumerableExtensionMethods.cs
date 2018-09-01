using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    internal static class EnumerableExtensionMethods
    {
        internal static T AddTo<T>(this T value, IList<T> values)
        {
            values.Add(value);
            return value;
        }

        /// <summary>
        /// Returns whether <paramref name="value"/> IsOneOf the <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static bool ContainedBy<T>(this T value, params T[] values) => values.Contains(value);

        /// <summary>
        /// Returns whether <paramref name="value"/> IsOneOf the <paramref name="values"/> using
        /// the <paramref name="equals"/> comparison.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="equals"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static bool ContainedBy<T>(this T value, Func<T, T, bool> equals, params T[] values)
            => values.Any(x => equals.Invoke(value, x));
    }
}
