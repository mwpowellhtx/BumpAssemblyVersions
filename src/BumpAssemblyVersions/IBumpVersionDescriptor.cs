using System.Collections.Generic;

namespace Bav
{
    internal interface IBumpVersionDescriptor
    {
        VersionKind Kind { get; }

        IVersionProvider MajorProvider { get; }

        IVersionProvider MinorProvider { get; }

        IVersionProvider PatchProvider { get; }

        IVersionProvider BuildProvider { get; }

        IVersionProvider ReleaseProvider { get; }

        bool CreateNew { get; set; }

        IEnumerable<IVersionProvider> VersionProviders { get; }
    }
}
