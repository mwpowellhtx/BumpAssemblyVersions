using System;
using System.Collections.Generic;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static DateTime;

    internal class BumpVersionDescriptor : IBumpVersionDescriptor
    {
        // TODO: TBD: once the providers are arranged, then the More Significant bits may be determined...
        private ITaskItem Item { get; }

        // TODO: TBD: starting with Timestamp specified here at the Descriptor level ...
        // TODO: TBD: it may be preferrable to draw this from the task/build event(s) instead, if possible ...
        private DateTime DescriptorTimestamp { get; } = Now;

        internal BumpVersionDescriptor(ITaskItem item)
        {
            Item = item;
        }

        // TODO: TBD: kind happens here agnostic of the Project file version, whether current msbuild/csharp, or legacy
        // TODO: TBD: will need to determine whether we're talking about csharp assembly attributes, or project file level Xml-style specs
        public VersionKind Kind { get; internal set; }

        private IVersionProvider Get(ref IVersionProvider provider, string request)
            => provider ?? (provider = Item.ToVersionProvider(request, DescriptorTimestamp));

        private IVersionProvider _majorProvider;

        public IVersionProvider MajorProvider => Get(ref _majorProvider, nameof(MajorProvider));

        private IVersionProvider _minorProvider;

        public IVersionProvider MinorProvider => Get(ref _minorProvider, nameof(MinorProvider));

        private IVersionProvider _patchProvider;

        public IVersionProvider PatchProvider => Get(ref _patchProvider, nameof(PatchProvider));

        private IVersionProvider _buildProvider;

        public IVersionProvider BuildProvider => Get(ref _buildProvider, nameof(BuildProvider));

        private IVersionProvider _releaseProvider;

        public IVersionProvider ReleaseProvider => Get(ref _releaseProvider, nameof(ReleaseProvider));

        /// <summary>
        /// Gets or sets whether to Create a New Version if one did not exist.
        /// </summary>
        public bool CreateNew { get; set; }

        // TODO: TBD: move this over to the Service?
        private static IVersionProvider InitMoreSignificant(IVersionProvider provider
            , params IVersionProvider[] moreSignificants)
        {
            ((VersionProviderBase) provider).MoreSignificantProviders = moreSignificants;
            return provider;
        }

        private IEnumerable<IVersionProvider> GetVersionProviders()
        {
            yield return InitMoreSignificant(MajorProvider);
            yield return InitMoreSignificant(MinorProvider, MajorProvider);
            yield return InitMoreSignificant(PatchProvider, MajorProvider, MinorProvider);
            yield return InitMoreSignificant(BuildProvider, PatchProvider, MajorProvider, MinorProvider);
            yield return InitMoreSignificant(ReleaseProvider, BuildProvider, PatchProvider, MajorProvider
                , MinorProvider);
        }

        public IEnumerable<IVersionProvider> VersionProviders => GetVersionProviders();
    }
}
