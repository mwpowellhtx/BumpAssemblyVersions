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
    using static ServiceMode;

    internal abstract class AssemblyInfoBumpVersionServiceBase : BumpVersionServiceBase
    {
        protected AssemblyInfoBumpVersionServiceBase(params IVersionProvider[] versionProviders)
            : base(versionProviders)
        {
        }

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
    }

    internal class AssemblyInfoBumpVersionServiceBase<T> : AssemblyInfoBumpVersionServiceBase
        where T : Attribute
    {
        private static readonly Type AttributeType = typeof(T);

        private static IEnumerable<Regex> GetAttributeRegexes()
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

        private IEnumerable<Regex> AttributeRegexes { get; } = GetAttributeRegexes().ToArray();

        // TODO: TBD: get set to consider how to receive specs from the end user...
        internal AssemblyInfoBumpVersionServiceBase(params IVersionProvider[] versionProviders)
            : base(versionProviders)
        {
        }

        // TODO: TBD: this one may serve for both the CSharp source file(s) as well as for the project file...
        // TODO: TBD: instead of reading by line, may read the whole file and split the lines out...
        // TODO: TBD: we would want the whole text for purposes of the CSPROJ Xml document parsing...
        /// <summary>
        /// Returns each of the lines from the <paramref name="assyInfoFullPath"/> sans any
        /// Carriage Returns or Lines Feeds.
        /// </summary>
        /// <param name="assyInfoFullPath"></param>
        /// <returns></returns>
        private static IEnumerable<string> ReadLinesFromFile(string assyInfoFullPath)
        {
            const FileMode mode = FileMode.Open;
            const FileAccess access = FileAccess.Read;
            const FileShare share = FileShare.ReadWrite;

            using (var fs = File.Open(assyInfoFullPath, mode, access, share))
            {
                using (var sr = new StreamReader(fs))
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
        }

        private static void WriteLinesToFile(string assyInfoFullPath, IEnumerable<string> lines)
        {
            const FileMode mode = FileMode.Create;
            const FileAccess access = FileAccess.Write;
            const FileShare share = FileShare.Read;

            using (var fs = File.Open(assyInfoFullPath, mode, access, share))
            {
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var line in lines)
                    {
                        sw.WriteLineAsync(line).Wait();
                    }
                }
            }
        }

        // TODO: TBD: this one may need to be promoted to first class assembly wide entity...
        private class BumpMatch
        {
            private readonly Match _match;

            internal string Line { get; private set; }

            internal bool IsMatch => _match?.Success ?? false;

            private BumpMatch(Match match)
            {
                _match = match;
            }

            internal static BumpMatch Create(Match match, string line)
                => new BumpMatch(match) {Line = line};
        }

        /// <summary>
        /// Returns whether the file on the other end of the <paramref name="assyInfoFullPath"/>
        /// Contains any of the <paramref name="lines"/> identified by the
        /// <see cref="AttributeRegexes"/> set of <see cref="Regex"/> pattern matchers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assyInfoFullPath"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private bool ContainsAttribute(string assyInfoFullPath, out IEnumerable<BumpMatch> lines)
            => (lines = ReadLinesFromFile(assyInfoFullPath).Select(
                        l => BumpMatch.Create(AttributeRegexes.FirstOrDefault(regex => regex.IsMatch(l))?.Match(l), l)
                    )
                ).Any(x => x.IsMatch);

        internal static bool VerifyServiceRequest(ServiceMode mode, IEnumerable<IVersionProvider> providers)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var p = providers.ToArray();

            if (SupportedAttributeTypes.All(type => type != AttributeType))
            {
                throw new InvalidOperationException(
                    $"The '{AttributeType.FullName}' is not among the supported types:"
                    + $" {Join(", ", SupportedAttributeTypes.Select(type => $"'{type.FullName}'"))}");
            }

            // TODO: TBD: may let "mode" be a decision driven by VersionProviders ...
            if (mode != NoElements || !p.Any())
            {
                return false;
            }

            if ((mode & ReleaseElements) != ReleaseElements)
            {
                return true;
            }

            if (!p.OfType<PreReleaseIncrementVersionProvider>().Any())
            {
                throw new InvalidOperationException(
                    $"Expected there to be a {typeof(PreReleaseIncrementVersionProvider).FullName}"
                    + $" among the '{nameof(VersionProviders)}'.");
            }

            if (p.Any() && !(p.Last() is PreReleaseIncrementVersionProvider))
            {
                throw new InvalidOperationException(
                    $"Expected the {typeof(PreReleaseIncrementVersionProvider).FullName} to be the"
                    + $" last of the enumerated '{nameof(VersionProviders)}'.");
            }

            // Clear to proceed, at least as far as up front Verification is concerned.
            return true;
        }

        /// <summary>
        /// Tries to Bump the Version contained by the <paramref name="assyInfoFullPath"/>
        /// indicated by the <typeparamref name="T"/> <see cref="Attribute"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assyInfoFullPath"></param>
        /// <returns></returns>
        internal bool TryBumpVersion(string assyInfoFullPath)
        {
            // Rule out several obvious scenarios prior to more intrusive vetting.
            if (!VerifyServiceRequest(Mode, VersionProviders))
            {
                return false;
            }

            /* Rule out any configuration related issues prior to touching any
             of the potentially involved files in any substantive way. */
            if (!ContainsAttribute(assyInfoFullPath, out var candidates))
            {
                return false;
            }

            const string version = nameof(version);

            // ReSharper disable once LoopCanBeConvertedToQuery it's not just this LINQ query, we want TODO more with it...
            // We aligned with the Candidates alread.
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var candidate in candidates.Where(x => x.IsMatch))
            {
                // TODO: TBD: instead of looking up the Regex all over again, just utilize the Match we discovered during the parse...

                // ReSharper disable once LoopCanBeConvertedToQuery ditto TODO...
                // Now we want to align with the Regexes as well.
                foreach (var regex in AttributeRegexes.Where(r => r.IsMatch(candidate.Line)))
                {
                    var match = regex.Match(candidate.Line);

                    var versionString = match.Groups[version].Value;

                    // TODO: TBD: this is where we want toe bits from the Match/Groups ...


                    // TODO: TBD: now that we actually have the Version String, then we must connect it somehow with the Version Provider(s)...

                    // TODO: TBD: this is where we must "parse" the version string, including comprehension of wildcards, possibly also comprehension of release labels, metadata, etc
                    // TODO: TBD: probably also with decision making driven by the VersionProviders...

                    return true;
                }
            }

            // ReSharper disable once PossibleMultipleEnumeration
            WriteLinesToFile(assyInfoFullPath, candidates.Select(x => x.Line));

            return false;
        }
    }
}
