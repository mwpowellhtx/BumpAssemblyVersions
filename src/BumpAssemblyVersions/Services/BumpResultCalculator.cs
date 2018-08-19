using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
    using static VersionProviderTemplateRegistry;
    using static RegexOptions;

    // TODO: TBD: at this point, I am leaning toward the "calculator" just being able to handle bumping the core (string) version
    /// <inheritdoc />
    public class BumpResultCalculator : IBumpResultCalculator
    {
        internal static string GetVersionRegexPattern()
        {
            /*
             * Went with an abbreviated notation here for brevity:
             * wc = wildcard
             * v = version
             * s = semantic
             * e = element
             * x = extended, meaning dot delimited
             */
            const string dot = @"\.";
            const string hyp = @"\-";
            var wc = $@"({dot}\*)";
            const string ve = @"(\d)+";
            var xve = $@"({dot}{ve})";
            var version = $@"(?<version>{ve}{xve}({wc}|{xve}{wc}?|{xve}{{2}})?)";
            // SE needs to be One Or More of Any of these Bits.
            var se = $@"[a-zA-Z\d{hyp}]+";
            var xse = $@"({dot}{se})";
            var semantic = $"(?<semantic>{se}{xse}*)";
            return $"(?<overall>{version}({hyp}{semantic})?)";
        }

        // ReSharper disable once StaticMemberInGenericType
        internal static readonly Regex VersionRegex = new Regex($"^({GetVersionRegexPattern()})$", Compiled);

        /// <inheritdoc />
        public IBumpVersionDescriptor Descriptor { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal BumpResultCalculator(IBumpVersionDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        /// <inheritdoc />
        public bool TryBumpResult(string given, IBumpResult result)
        {
            // TODO: TBD: provide hooks whether to 1) Support Wildcard, and 2) Supports Pre-Release Semantic Version.
            const char dot = '.';
            const char wildcard = '*';

            var match = VersionRegex.Match(given);

            // ReSharper disable once LocalNameCapturedOnly
            string version;

            if (!(match.Groups.HasGroupName(nameof(version))
                  || match.Groups[nameof(version)].Success))
            {
                return result.DidBump;
            }

            /* Capture this once and once only across the breadth of the request. Yes, we want
             to capture a whole new collection, because any prior matches in the same file will
             also need to have been bumped in a consistent manner, including any More Significant
             Providers in prior sequences. */

            var versionProviders = Descriptor.VersionProviders.ToArray();

            // TODO: TBD: should "wildcard" be a special field? potentially, it could even be its own Version Provider, i.e. WildcardVersionProvider ...
            // TODO: TBD: which would be a long-handed way of saying "*" ... but it might make better sense than reading around the wildcard field ...
            var versionElements = (result.OldVersionString = match.Groups[nameof(version)].Value).Split(dot);
            var hasWildcard = versionElements.Any(x => x == $"{wildcard}");

            var changedElements = versionElements
                .Take(versionElements.Length - (hasWildcard ? 1 : 0))
                .Zip(versionProviders.Take(versionElements.Length - (hasWildcard ? 1 : 0))
                    , (x, p) => p.TryChange(x, out var y) ? y : x).ToArray();

            result.VersionString = version = Join($"{dot}", changedElements);

            if (hasWildcard)
            {
                // ReSharper disable once RedundantAssignment
                result.VersionString = version = Join($"{dot}", version, $"{wildcard}");
            }

            /* TODO: TBD: there is a corner case in here whereby we would not necessarily want a
             Wildcard to coexist with a Semantic, but we will leave that go for the time being... */
            string semantic;

            // ReSharper disable once InvertIf
            // Having the Group does not necessarily mean Successful Match.
            if (match.Groups.HasGroupName(nameof(semantic))
                && match.Groups[nameof(semantic)].Success)
            {
                semantic = result.OldSemanticString = match.Groups[nameof(semantic)].Value;

                /* Semantic simply falls through as-is when there is no Provider given.
                 However, not being given a Provider is effectively a NoOp. */
                var p = versionProviders.OfType<PreReleaseIncrementVersionProvider>().SingleOrDefault()
                        ?? (IVersionProvider) Registry.NoOp;

                /* The difference here is subtle. We only want to Pre-Release to Bump when there
                 * has been an actual change. However, No-op should pass through regardless. */
                if (p is PreReleaseIncrementVersionProvider preRelease
                    && preRelease.TryChange(semantic, out var newSemantic))
                {
                    result.SemanticString = newSemantic;
                }
                else
                {
                    p.TryChange(semantic, out var noOpSemantic);
                    result.SemanticString = noOpSemantic;
                }
            }

            return result.DidBump;
        }

        /// <summary>
        /// Gets whether IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <inheritdoc />
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
            }

            IsDisposed = true;
        }
    }
}
