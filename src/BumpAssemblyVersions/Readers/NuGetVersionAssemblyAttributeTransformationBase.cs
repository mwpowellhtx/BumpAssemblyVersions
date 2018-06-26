using System;
using System.Linq;

namespace Bav
{
    using NuGet.Versioning;

    internal abstract class NuGetVersionAssemblyAttributeTransformationBase<TAttribute>
        : SemanticVersionAssemblyAttributeTransformationBase<TAttribute, NuGetVersion>
        where TAttribute : Attribute
    {
        protected NuGetVersionAssemblyAttributeTransformationBase(string sourcePath)
            : base(sourcePath)
        {
        }

        protected override NuGetVersion Parse(string s) => NuGetVersion.Parse(s);

        public override bool TryChangeVersioning(params IVersionProvider[] providers)
        {
            if (!base.TryChangeVersioning(providers))
            {
                return false;
            }

            var versionBeforeString = Match.Value;
            var versionBefore = Parse(versionBeforeString);

            // Connect the Parsed Release Labels with the Provider.
            var preReleaseProvider = (PreReleaseIncrementVersionProvider) providers
                .SingleOrDefault(provider => provider is PreReleaseIncrementVersionProvider);

            if (preReleaseProvider != null)
            {
                preReleaseProvider.Label = versionBefore.Release;
            }

            // TODO: TBD: see this whole mess here; https://github.com/BalassaMarton/MSBump/blob/master/MSBump/BumpVersion.cs#L109
            // This is where "Releaselabels" (so-called) are handled, parsed, otherwise juggled...
            var labels = versionBefore.ReleaseLabels.ToList();

            // TODO: TBD: perform the bump... this should involve some degree of alignment with bits for providers, the nugetversion, etc...
            // TODO: TBD: relay the necessary detail back to the NuGetVersion ...

            // TODO: TBD: also there is the matter what to do with null-origin fields? i.e. what happens when patch? build? etc, are nowhere to be found in the source usage?

            var versionAfter = Parse($"{versionBefore}");

            return false;
        }
    }
}
