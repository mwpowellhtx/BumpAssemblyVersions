using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using Xunit;

    internal static class AttributeRegexExtensionMethods
    {
        internal static Regex FilterMatch(this IEnumerable<Regex> regexes, string given, bool shouldMatch)
        {
            Assert.NotNull(regexes);
            Assert.NotNull(given);
            Assert.NotEmpty(given);

            void VerifyFiltered(Regex filtered)
                => (shouldMatch ? (Action<object>) Assert.NotNull : Assert.Null).Invoke(filtered);

            var regex = regexes.SingleOrDefault(r => r.IsMatch(given));

            VerifyFiltered(regex);

            return regex;
        }

        internal static bool TryVerifyMatch(this Regex regex, string given, string expectedVersion, bool shouldMatch)
        {
            if (!shouldMatch)
            {
                return false;
            }

            Assert.NotNull(regex);

            var match = regex.Match(given);

            const string version = nameof(version);

            Assert.NotNull(match);
            Assert.True(match.Success);
            Assert.True(match.Groups.HasGroupName(version));

            var actualVersion = match.Groups[version].Value;

            Assert.Equal(expectedVersion, actualVersion);

            return true;
        }
    }
}
