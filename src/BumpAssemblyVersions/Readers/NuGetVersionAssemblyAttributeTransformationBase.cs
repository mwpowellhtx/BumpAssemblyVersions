using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using NuGet.Versioning;
    using static String;
    using static RegexOptions;

    internal abstract class NuGetVersionAssemblyAttributeTransformationBase<TAttribute>
        : SemanticVersionAssemblyAttributeTransformationBase<TAttribute, NuGetVersion>
        where TAttribute : Attribute
    {
        protected NuGetVersionAssemblyAttributeTransformationBase(string sourcePath)
            : base(sourcePath)
        {
        }

        protected override NuGetVersion Parse(string s) => NuGetVersion.Parse(s);

        private static string GetVersionRegexPattern()
        {
            const string dot = @"\.";
            const string hypen = @"\-";
            const string plus = @"\+";
            const string idstart = @"[1-9a-zA-Z\-]";
            const string id = @"[\da-zA-Z\-]";
            const string elem = @"\d+";

            var version = $@"{elem}({dot}{elem}){{0,3}}";
            var release = $@"({idstart}{id}*)[{dot}({idstart}{id}*)]+";
            var metadata = release;

            return $@"^(?<{nameof(version)}>{version})"
                   + $@"({hypen}(?<{nameof(release)}>{release}))?"
                   + $@"({plus}(?<{nameof(metadata)}>{metadata}))?$"
                ;
        }

        private readonly Regex _versionRegex = new Regex(GetVersionRegexPattern(), Compiled);

        private bool TryParseNaiveVersionElements(string s, out string version, out string release, out string metadata)
        {
            string GetGroupValueOrDefault(Match m, string groupName, string defaultValue = null)
                => m.Success && m.Groups.HasGroupName(groupName)
                    ? m.Groups[groupName].Value
                    : defaultValue ?? Empty;

            var match = _versionRegex.Match(s);

            version = GetGroupValueOrDefault(match, nameof(version));
            release = GetGroupValueOrDefault(match, nameof(release));
            metadata = GetGroupValueOrDefault(match, nameof(metadata));

            // Release and Metadata are Optional from this POV.
            return match.Success && !IsNullOrEmpty(version);
        }

        private static string GetReleasePattern() => @"^(?<label>[a-zA-Z]+)(?<count>\d+)$";

        private readonly Regex _releaseRegex = new Regex(GetReleasePattern(), Compiled);

        private bool TryParseReleaseLabel(string s, out string label, out int count)
        {
            T GetGroupValue<T>(Match m, string groupName, Func<string, T> convert)
                => m.Success && m.Groups.HasGroupName(groupName)
                    ? convert(m.Groups[groupName].Value)
                    : default(T);

            var match = _releaseRegex.Match(s);

            label = GetGroupValue(match, nameof(label), x => x);
            count = GetGroupValue(match, nameof(count), int.Parse);

            return match.Success;
        }

        public override bool TryChangeVersioning(params IVersionProvider[] providers)
        {
            if (!base.TryChangeVersioning(providers))
            {
                return false;
            }

            const char dot = '.';
            const char hyphen = '-';

            var versionBeforeString = Match.Value;
            var versionBefore = Parse(versionBeforeString);

            if (!TryParseNaiveVersionElements(versionBeforeString
                , out var version, out var release, out var metadata)
            )
            {
                return false;
            }

            // TODO: TBD: perform the bump... this should involve some degree of alignment with bits for providers, the nugetversion, etc...
            // TODO: TBD: relay the necessary detail back to the NuGetVersion ...
            // TODO: TBD: also there is the matter what to do with null-origin fields? i.e. what happens when patch? build? etc, are nowhere to be found in the source usage?
            var versionElements = version.Split(new[] {$"{dot}"}, StringSplitOptions.None)
                .Select(x => IsNullOrEmpty(x) ? Empty : x).ToList();

            // Align the Version Elements with the corresponding Element Providers.
            var elementProviders = providers.Where(provider => !(provider is PreReleaseIncrementVersionProvider)).ToArray();
            while (versionElements.Count < elementProviders.Length)
            {
                versionElements.Add(Empty);
            }

            // Then Apply those Providers to each of the Aligned Elements.
            var changedElements = elementProviders
                .Zip(versionElements
                    , (provider, element) => provider.TryChange(element, out var changedElement) ? changedElement : "0");

            var hasRelease = !IsNullOrEmpty(release);

            // Connect the Parsed Release Labels with the Provider.
            var preReleaseProvider = (PreReleaseIncrementVersionProvider) providers
                .SingleOrDefault(provider => provider is PreReleaseIncrementVersionProvider);

            string newRelease = null;

            // TODO: TBD: see this whole mess here; https://github.com/BalassaMarton/MSBump/blob/master/MSBump/BumpVersion.cs#L109
            if (!IsNullOrEmpty(release)
                && preReleaseProvider != null
                && TryParseReleaseLabel(release, out var label, out _))
            {
                preReleaseProvider.Label = label;

                // TODO: TBD: may throw that Pre-Release could not change...
                if (!preReleaseProvider.TryChange(release, out newRelease))
                {
                    return false;
                }
            }

            // Last but not least we ignore the Metadata field.
            // TODO: TBD: or, we may throw an exception, metadata unsupported, etc...

            var versionAfterString = hasRelease == false
                ? $"{Join($"{dot}", changedElements)}"
                : $"{Join($"{dot}", changedElements)}{hyphen}{newRelease}";

            var versionAfter = Parse(versionAfterString);

            return false;
        }
    }
}
