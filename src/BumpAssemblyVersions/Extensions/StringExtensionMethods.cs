using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using static String;

    internal static class StringExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="width"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string PadLeft(this string s, int width, string x)
        {
            while (s.Length < width)
            {
                s = $"{x}{s}";
            }

            return s;
        }

        /// <summary>
        /// Trims any Leading characters found in <paramref name="chs"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="chs"></param>
        /// <returns></returns>
        public static string TrimLeading(this string s, params char[] chs)
            => s.SkipWhile(x => chs.Any() && chs.Any(y => x == y))
                .Aggregate(Empty, (g, ch) => $"{g}{ch}");

        /// <summary>
        /// Splits the <see cref="String"/> <paramref name="s"/> with the First appearance
        /// of any of the <paramref name="separator"/> <see cref="Char"/> values.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitFirst(this string s, params char[] separator)
        {
            var index = separator.Select(ch => (int?) s.IndexOf(ch)).FirstOrDefault(i => i >= 0);

            if (index == null)
            {
                yield return s;
                yield return Empty;
                yield break;
            }

            yield return s.Substring(0, index.Value);
            yield return s.Substring(index.Value + 1, s.Length - index.Value - 1);
        }
    }
}
