using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static DescriptorExtensionMethods.MetadataNames;
    using static String;

    /// <summary>
    /// Provides several codified hooks connecting with user facing Build Project level
    /// Items Specifications.
    /// </summary>
    internal static class DescriptorExtensionMethods
    {
        internal static class MetadataNames
        {
            internal const string UseUtc = nameof(UseUtc);
            internal const string CreateNew = nameof(CreateNew);
            internal const string IncludeWildcard = nameof(IncludeWildcard);
            internal const string DefaultVersion = nameof(DefaultVersion);
        }

        /// <summary>
        /// Gets the <see cref="BumpVersionDescriptor"/> corresponding to the
        /// <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static BumpVersionDescriptor ToDescriptor(this ITaskItem item)
        {
            // TODO: TBD: To Descriptor? used in what way?
            var descriptor = new BumpVersionDescriptor(item);

            // TODO: TBD: may combine these into a common area...
            void SetPropertyFromMetadata<T>(string metadataName, Func<string, T> convert
                , Action<BumpVersionDescriptor, T> setter)
            {
                if (item.HasMetadataName(metadataName))
                {
                    setter(descriptor, convert(item.GetMetadata(metadataName)));
                }
            }

            SetPropertyFromMetadata(UseUtc, bool.Parse, (d, x) => d.UseUtc = x);
            SetPropertyFromMetadata(CreateNew, bool.Parse, (d, x) => d.CreateNew = x);
            SetPropertyFromMetadata(IncludeWildcard, bool.Parse, (d, x) => d.IncludeWildcard = x);
            SetPropertyFromMetadata(DefaultVersion, s => s, (d, s) => d.DefaultVersion = s);

            // TODO: TBD: from "VersionKind" we need to decide what the context is, whether an AssemblyInfo context, or for the new 2017 CSPROJ Xml format.
            var kind = item.ItemSpec.ToNullableVersionKind();

            if (kind == null)
            {
                IEnumerable<string> GetVersionKindListing()
                    => from k in Enum.GetValues(typeof(VersionKind)).OfType<VersionKind>()
                        select $"'{k}'";

                throw new InvalidOperationException(
                    $"Expecting '{nameof(ITaskItem.ItemSpec)}' to be among: {{ {Join(", ", GetVersionKindListing())} }}"
                );
            }

            descriptor.Kind = kind.Value;

            return descriptor;
        }

        //private static void SetProviderTimestamp(this IVersionProvider provider
        //    , DateTime timestamp, bool? useUtc = null)
        //    => provider?.SetTimestamp(timestamp, useUtc);

        // TODO: TBD: I know what I was driving at with these, but it may not be the best placement for it...
        //internal static void SetMajorProviderTimestamp(this BumpVersionDescriptor descriptor
        //    , DateTime timestamp, bool? useUtc = null)
        //    => descriptor.MajorProvider.SetProviderTimestamp(timestamp, useUtc);

        //internal static void SetMinorProviderTimestamp(this BumpVersionDescriptor descriptor
        //    , DateTime timestamp, bool? useUtc = null)
        //    => descriptor.MinorProvider.SetProviderTimestamp(timestamp, useUtc);

        //internal static void SetPatchProviderTimestamp(this BumpVersionDescriptor descriptor
        //    , DateTime timestamp, bool? useUtc = null)
        //    => descriptor.PatchProvider.SetProviderTimestamp(timestamp, useUtc);

        //internal static void SetBuildProviderTimestamp(this BumpVersionDescriptor descriptor
        //    , DateTime timestamp, bool? useUtc = null)
        //    => descriptor.BuildProvider.SetProviderTimestamp(timestamp, useUtc);

        //internal static void SetReleaseProviderTimestamp(this BumpVersionDescriptor descriptor
        //    , DateTime timestamp, bool? useUtc = null)
        //    => descriptor.ReleaseProvider.SetProviderTimestamp(timestamp, useUtc);

        ///// <summary>
        ///// Call in order to Set the <see cref="IVersionProvider.MayReset"/> value. Null
        ///// propagation usage dictates that we are guaranteed an instance.
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="mayReset"></param>
        //private static void SetProviderMayReset(this IVersionProvider provider, bool mayReset)
        //    => provider.MayReset = mayReset;

        //internal static void SetMajorProviderMayReset(this BumpVersionDescriptor descriptor, bool mayReset)
        //    => descriptor.MajorProvider?.SetProviderMayReset(mayReset);

        //internal static void SetMinorProviderMayReset(this BumpVersionDescriptor descriptor, bool mayReset)
        //    => descriptor.MinorProvider?.SetProviderMayReset(mayReset);

        //internal static void SetPatchProviderMayReset(this BumpVersionDescriptor descriptor, bool mayReset)
        //    => descriptor.PatchProvider?.SetProviderMayReset(mayReset);

        //internal static void SetBuildProviderMayReset(this BumpVersionDescriptor descriptor, bool mayReset)
        //    => descriptor.BuildProvider?.SetProviderMayReset(mayReset);

        //internal static void SetReleaseProviderMayReset(this BumpVersionDescriptor descriptor, bool mayReset)
        //    => descriptor.ReleaseProvider?.SetProviderMayReset(mayReset);
    }
}
