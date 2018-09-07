using System.Collections.Generic;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBumpVersionDescriptor
    {
        /// <summary>
        /// 
        /// </summary>
        VersionKind Kind { get; }

        /// <summary>
        /// 
        /// </summary>
        IVersionProvider MajorProviderTemplate { get; }

        /// <summary>
        /// 
        /// </summary>
        IVersionProvider MinorProviderTemplate { get; }

        /// <summary>
        /// 
        /// </summary>
        IVersionProvider PatchProviderTemplate { get; }

        /// <summary>
        /// 
        /// </summary>
        IVersionProvider BuildProviderTemplate { get; }

        /// <summary>
        /// 
        /// </summary>
        IVersionProvider ReleaseProviderTemplate { get; }

        /// <summary>
        /// Gets or sets whether to UseUtc.
        /// </summary>
        bool? UseUtc { get; set; }

        /// <summary>
        /// Gets or sets whether to CreateNew.
        /// </summary>
        bool CreateNew { get; set; }

        /// <summary>
        /// Gets or sets whether to MayReset.
        /// </summary>
        bool MayReset { get; set; }

        /// <summary>
        /// Gets or sets whether to IncludeWildcard.
        /// </summary>
        bool IncludeWildcard { get; set; }

        /// <summary>
        /// Gets or sets the DefaultVersion.
        /// </summary>
        string DefaultVersion { get; set; }

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
