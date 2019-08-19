using System.Collections.Generic;

namespace Bav
{
    using Microsoft.Build.Framework;

    /// <summary>
    /// 
    /// </summary>
    public interface IBumpVersionDescriptor
    {
        /// <summary>
        /// Gets the BumpPath. Corresponds to the <see cref="ITaskItem.ItemSpec"/>
        /// used to specify the item in the first place.
        /// </summary>
        string BumpPath { get; }

        /// <summary>
        /// Gets the Kind of Version.
        /// </summary>
        VersionKind Kind { get; }

        /// <summary>
        /// Gets the Major Provider Template.
        /// </summary>
        IVersionProvider MajorProviderTemplate { get; }

        /// <summary>
        /// Gets the Minor Provider Template.
        /// </summary>
        IVersionProvider MinorProviderTemplate { get; }

        /// <summary>
        /// Gets the Patch Provider Template.
        /// </summary>
        IVersionProvider PatchProviderTemplate { get; }

        /// <summary>
        /// Gets the Build Provider Template.
        /// </summary>
        IVersionProvider BuildProviderTemplate { get; }

        /// <summary>
        /// Gets the Release Provider Template.
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
