using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using static ServiceMode;

    internal abstract class BumpVersionServiceBase
    {
        protected internal ServiceMode Mode { get; set; } = VersionElements;

        private IEnumerable<IVersionProvider> _versionProviders;

        private static IEnumerable<IVersionProvider> GetDefaultVersionProviders()
        {
            yield break;
        }

        protected internal IEnumerable<IVersionProvider> VersionProviders
        {
            get => _versionProviders ?? (_versionProviders = GetDefaultVersionProviders());
            set => _versionProviders = (value ?? GetDefaultVersionProviders()).ToArray();
        }

        protected BumpVersionServiceBase(params IVersionProvider[] versionProviders)
        {
            VersionProviders = versionProviders;
        }
    }
}
