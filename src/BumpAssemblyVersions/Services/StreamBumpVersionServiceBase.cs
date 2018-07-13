using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
    using static RegexOptions;

    internal abstract class StreamBumpVersionServiceBase<T> : BumpVersionServiceBase, IDisposable
        where T : Attribute
    {
        // ReSharper disable once StaticMemberInGenericType
        private static IEnumerable<Type> _supportedAttributeTypes;

        protected static IEnumerable<Type> SupportedAttributeTypes
        {
            get
            {
                IEnumerable<Type> GetAll()
                {
                    // TODO: TBD: that I know of... are there any others?
                    yield return typeof(AssemblyVersionAttribute);
                    yield return typeof(AssemblyFileVersionAttribute);
                    yield return typeof(AssemblyInformationalVersionAttribute);
                }

                return _supportedAttributeTypes ?? (_supportedAttributeTypes = GetAll().ToArray());
            }
        }

        protected static readonly Type AttributeType = typeof(T);

        protected static IEnumerable<Regex> GetAttributeRegexes()
        {
            // We will return the naive pattern matching for faster identification.
            string GetRegexPattern(string attribName)
                => $@"\[assembly\: {attribName}\(""(?<version>[a-zA-Z\d\.\-\+\*]+)""\)\]";
            // TODO: TBD: separate out the version bits, including potential for wildcard, from the release label identifying bits, from any metadata identifying bits...

            var longName = AttributeType.Name;
            var shortName = longName.Substring(0, longName.Length - nameof(Attribute).Length);

            yield return new Regex(GetRegexPattern(longName), Compiled);
            yield return new Regex(GetRegexPattern(shortName), Compiled);
        }

        protected internal IEnumerable<Regex> AttributeRegexes { get; } = GetAttributeRegexes().ToArray();

        protected StreamBumpVersionServiceBase(params IVersionProvider[] versionProviders)
            : base(versionProviders)
        {
        }

        /// <summary>
        /// Returns each of the lines from the <paramref name="stream"/> sans any Carriage
        /// Returns or Lines Feeds.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static IEnumerable<string> ReadLinesFromStream(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                const string cr = @"\r";
                const string lf = @"\n";

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine() ?? Empty;
                    yield return line.Replace(cr, Empty).Replace(lf, Empty);
                }
            }
        }

        /// <summary>
        /// Writes the <paramref name="lines"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lines"></param>
        protected static void WriteLinesToStream(Stream stream, IEnumerable<string> lines)
        {
            using (var sw = new StreamWriter(stream))
            {
                foreach (var line in lines.ToArray())
                {
                    sw.WriteLineAsync(line).Wait();
                }
            }
        }

        // TODO: TBD: this one may need to be promoted to first class assembly wide entity...
        protected class BumpMatch
        {
            private readonly Match _match;

            internal string Line { get; private set; }

            internal bool IsMatch => _match?.Success ?? false;

            private BumpMatch(Match match)
            {
                _match = match;
            }

            internal static BumpMatch Create(Match match, string line)
                => new BumpMatch(match) { Line = line };
        }

        /// <summary>
        /// Returns whether the <paramref name="lines"/> Contains Any of the
        /// <paramref name="results"/> identified by the <see cref="AttributeRegexes"/> set of
        /// <see cref="Regex"/> pattern matchers.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        protected bool ContainsAttribute(IEnumerable<string> lines, out IEnumerable<BumpMatch> results)
            => (results = lines.Select(
                        l => BumpMatch.Create(AttributeRegexes.FirstOrDefault(regex => regex.IsMatch(l))?.Match(l), l)
                    )
                ).Any(x => x.IsMatch);

        protected bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
