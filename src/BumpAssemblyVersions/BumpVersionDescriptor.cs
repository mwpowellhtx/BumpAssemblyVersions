using System;
using System.Collections.Generic;
using System.Linq;

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

        private IVersionProvider _majorProviderTemplate;

        public IVersionProvider MajorProviderTemplate => Get(
            ref _majorProviderTemplate, nameof(MajorProviderTemplate));

        private IVersionProvider _minorProviderTemplate;

        public IVersionProvider MinorProviderTemplate => Get(
            ref _minorProviderTemplate, nameof(MinorProviderTemplate));

        private IVersionProvider _patchProviderTemplate;

        public IVersionProvider PatchProviderTemplate => Get(
            ref _patchProviderTemplate, nameof(PatchProviderTemplate));

        private IVersionProvider _buildProviderTemplate;

        public IVersionProvider BuildProviderTemplate => Get(
            ref _buildProviderTemplate, nameof(BuildProviderTemplate));

        private IVersionProvider _releaseProviderTemplate;

        public IVersionProvider ReleaseProviderTemplate => Get(
            ref _releaseProviderTemplate, nameof(ReleaseProviderTemplate));

        /// <summary>
        /// Gets or sets whether to Create a New Version if one did not exist.
        /// </summary>
        public bool CreateNew { get; set; }

        private IEnumerable<IVersionProvider> _versionProviderTemplates;

        /// <inheritdoc />
        public IEnumerable<IVersionProvider> VersionProviderTemplates
        {
            get
            {
                IEnumerable<IVersionProvider> GetAll()
                {
                    yield return MajorProviderTemplate;
                    yield return MinorProviderTemplate;
                    yield return PatchProviderTemplate;
                    yield return BuildProviderTemplate;
                    yield return ReleaseProviderTemplate;
                }

                return _versionProviderTemplates ?? (_versionProviderTemplates = GetAll().ToArray());
            }
        }

        /// <inheritdoc />
        public IEnumerable<IVersionProvider> VersionProviders
        {
            get
            {
                // Returns the Taken Values ThroughIndex.
                IEnumerable<T> Take<T>(IEnumerable<T> values, int throughIndex)
                {
                    for (var i = 0; i < throughIndex; i++)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        yield return values.ElementAt(i);
                    }
                }

                VersionProviderBase GetCurrent(VersionProviderBase currentProvider
                    , params IVersionProvider[] moreSignificant)
                {
                    currentProvider.MoreSignificantProviders = moreSignificant.ToArray();
                    return currentProvider;
                }

                var clonedProviders = VersionProviderTemplates.Select(
                    template => (VersionProviderBase) template.Clone()).ToList();

                foreach (var clonedProvider in clonedProviders)
                {
                    var index = clonedProviders.IndexOf(clonedProvider);
                    yield return GetCurrent(clonedProvider, Take(clonedProviders, index).ToArray<IVersionProvider>());
                }
            }
        }
    }
}
