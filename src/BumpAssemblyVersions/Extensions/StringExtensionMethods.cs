using System;
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
    }
}
