using System.Collections.Generic;

namespace Bav
{
    using static ServiceMode;

    internal abstract class BumpVersionServiceBase
    {
        protected internal ServiceMode Mode { get; set; } = VersionElements;

        protected IEnumerable<IVersionProvider> VersionProviders { get; }

        protected BumpVersionServiceBase(params IVersionProvider[] versionProviders)
        {
            VersionProviders = versionProviders;
        }
    }
}
