﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static DateTime;
    using static VersionKind;

    internal partial class BumpVersionDescriptor : IBumpVersionDescriptor
    {
        public Guid Id { get; } = Guid.NewGuid();

        // TODO: TBD: starting with Timestamp specified here at the Descriptor level ...
        // TODO: TBD: it may be preferable to draw this from the task/build event(s) instead, if possible ...
        protected DateTime DescriptorTimestamp { private get; set; } = Now;

        /// <inheritdoc />
        public string BumpPath { get; }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="item">Once arranged we no longer require the <see cref="ITaskItem"/>.</param>
        /// <see cref="DescriptorTimestamp"/>
        internal BumpVersionDescriptor(ITaskItem item)
        {
            BumpPath = item.ItemSpec;

            // Gets the Provider Template given the Item.
            IVersionProvider Get(string request) => item.ToVersionProvider(request, DescriptorTimestamp);

            MajorProviderTemplate = Get(nameof(MajorProviderTemplate));
            MinorProviderTemplate = Get(nameof(MinorProviderTemplate));
            PatchProviderTemplate = Get(nameof(PatchProviderTemplate));
            BuildProviderTemplate = Get(nameof(BuildProviderTemplate));
            ReleaseProviderTemplate = Get(nameof(ReleaseProviderTemplate));
        }

        // TODO: TBD: kind happens here agnostic of the Project file version, whether current msbuild/csharp, or legacy
        // TODO: TBD: will need to determine whether we're talking about csharp assembly attributes, or project file level Xml-style specs
        public VersionKind Kind { get; internal set; } = None;

        // TODO: TBD: consider exposing an internal setter?
        // TODO: TBD: possibly fixture-ize the descriptor?
        /// <inheritdoc />
        public IVersionProvider MajorProviderTemplate { get; }

        /// <inheritdoc />
        public IVersionProvider MinorProviderTemplate { get; }

        /// <inheritdoc />
        public IVersionProvider PatchProviderTemplate { get; }

        /// <inheritdoc />
        public IVersionProvider BuildProviderTemplate { get; }

        /// <inheritdoc />
        public IVersionProvider ReleaseProviderTemplate { get; }

        /// <inheritdoc />
        public bool CreateNew { get; set; }

        /// <inheritdoc />
        public bool MayReset { get; set; }

        /// <inheritdoc />
        public bool IncludeWildcard { get; set; }

        /// <inheritdoc />
        public string DefaultVersion { get; set; }

        /// <inheritdoc />
        public bool? UseUtc { get; set; }

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

                VersionProviderBase CloneCurrent(IVersionProvider currentProvider)
                {
                    var clone = (VersionProviderBase) currentProvider.Clone();
                    clone.Descriptor = this;
                    clone.SetTimestamp(DescriptorTimestamp, UseUtc);
                    return clone;
                }

                VersionProviderBase GetCurrent(VersionProviderBase currentProvider
                    , params IVersionProvider[] moreSignificant)
                {
                    currentProvider.MoreSignificantProviders = moreSignificant.ToArray();
                    return currentProvider;
                }

                var clonedProviders = VersionProviderTemplates.Select(CloneCurrent).ToList();

                foreach (var clonedProvider in clonedProviders)
                {
                    var index = clonedProviders.IndexOf(clonedProvider);
                    yield return GetCurrent(clonedProvider, Take(clonedProviders, index).ToArray<IVersionProvider>());
                }
            }
        }
    }
}
