using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bav
{
    using static String;

    internal class AssemblyInfoBumpVersionService<T> : StreamBumpVersionServiceBase<T>
        where T : Attribute
    {
        // TODO: TBD: get set to consider how to receive specs from the end user...
        internal AssemblyInfoBumpVersionService(params IVersionProvider[] versionProviders)
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
                return ReadLinesFromStream(fs);
            }
        }

        private static void WriteLinesToFile(string assyInfoFullPath, IEnumerable<string> lines)
        {
            const FileMode mode = FileMode.Create;
            const FileAccess access = FileAccess.Write;
            const FileShare share = FileShare.Read;

            using (var fs = File.Open(assyInfoFullPath, mode, access, share))
            {
                WriteLinesToStream(fs, lines.ToArray());
            }
        }

        /// <summary>
        /// Returns whether the file on the other end of the <paramref name="assyInfoFullPath"/>
        /// Contains any of the <paramref name="results"/>.
        /// </summary>
        /// <param name="assyInfoFullPath"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        /// <see cref="StreamBumpVersionServiceBase{T}.ContainsAttribute"/>
        private bool ContainsAttribute(string assyInfoFullPath, out IEnumerable<BumpMatch> results)
            => ContainsAttribute(ReadLinesFromFile(assyInfoFullPath), out results);

        private static bool VerifyServiceRequest(ServiceMode mode, IEnumerable<IVersionProvider> providers)
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
            if (mode != ServiceMode.NoElements || !p.Any())
            {
                return false;
            }

            if ((mode & ServiceMode.ReleaseElements) != ServiceMode.ReleaseElements)
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