using System.Linq;

namespace Bav
{
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
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string TrimLeading(this string s, char ch)
        {
            while (s.Any() && s.First() == ch)
            {
                s = s.Substring(1, s.Length - 1);
            }

            return s;
        }
    }
}
