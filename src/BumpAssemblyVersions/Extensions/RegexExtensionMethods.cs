using System;
using System.Text.RegularExpressions;

namespace Bav
{
    /// <summary>
    /// Provides a couple of helpful Extension Methods for use with <see cref="Regex"/> based
    /// concerns.
    /// </summary>
    internal static class RegexExtensionMethods
    {
        /// <summary>
        /// Returns whether Any of the <paramref name="groups"/> matches the
        /// <paramref name="predicate"/>.
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public static bool Any(this GroupCollection groups, Func<Group, bool> predicate = null)
        {
            predicate = predicate ?? (g => true);

            foreach (Group g in groups)
            {
                if (predicate(g))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether <paramref name="groups"/> Has the <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static bool HasGroupName(this GroupCollection groups, string groupName)
        {
            try
            {
                var g = groups[groupName];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
