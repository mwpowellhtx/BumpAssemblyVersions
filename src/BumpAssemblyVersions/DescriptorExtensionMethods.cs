using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
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
        /// <param name="log"></param>
        /// <returns></returns>
        internal static BumpVersionDescriptor ToDescriptor(this ITaskItem item
            , TaskLoggingHelper log = null)
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

            // TODO: TBD: look into it, see if there is a way that Property Groups could be used instead, i.e. that do not appear as "project items" ...
            // TODO: TBD: they must include attributes, etc...
            VersionKind ParseVersionKind(string s)
            {
                var kind = s.ToNullableVersionKind();

                if (kind != null)
                {
                    return kind.Value;
                }

                IEnumerable<string> GetVersionKindListing()
                    => from k in Enum.GetValues(typeof(VersionKind)).OfType<VersionKind>()
                        select $"'{k}'";

                throw new InvalidOperationException(
                    $"Expecting '{typeof(ITaskItem).FullName + '.' + nameof(ITaskItem.ItemSpec)}' to be among: {{ {Join(", ", GetVersionKindListing())} }}"
                );
            }

            descriptor.Kind = ParseVersionKind(item.ItemSpec);

#if TASK_LOGGING_HELPER_DIAGNOSTICS
            log?.LogWarning($"Parsed version kind '{descriptor.Kind}'.");
#endif

            return descriptor;
        }
    }
}
