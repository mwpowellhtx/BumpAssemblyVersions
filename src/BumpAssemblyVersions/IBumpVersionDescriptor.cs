using System.Collections.Generic;

namespace Bav
{
    internal interface IBumpVersionDescriptor
    {
        VersionKind Kind { get; }

        IVersionProvider MajorProviderTemplate { get; }

        IVersionProvider MinorProviderTemplate { get; }

        IVersionProvider PatchProviderTemplate { get; }

        IVersionProvider BuildProviderTemplate { get; }

        IVersionProvider ReleaseProviderTemplate { get; }

        bool CreateNew { get; set; }

        /// <summary>
        /// Gets the set of <see cref="IVersionProvider"/> Templates. This collection should
        /// be static based on the specified Descriptor Templates.
        /// </summary>
        IEnumerable<IVersionProvider> VersionProviderTemplates { get; }

        /// <summary>
        /// Gets the set of <see cref="IVersionProvider"/> instances cloned from the
        /// <see cref="VersionProviderTemplates"/>. This is a more dynamic list and should
        /// recalculate the <see cref="IVersionProvider.MoreSignificantProviders"/> every time.
        /// This should ostensibly connect with whatever calling context parser is responsible
        /// for considering each qualifying match.
        /// </summary>
        IEnumerable<IVersionProvider> VersionProviders { get; }
    }
}
