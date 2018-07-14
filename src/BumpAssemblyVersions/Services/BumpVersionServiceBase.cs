using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    public abstract class BumpVersionServiceBase : IBumpVersionService
    {
        /// <summary>
        /// Gets the Mode.
        /// </summary>
        public ServiceMode Mode { get; protected internal set; } = VersionElements;

        private IEnumerable<IVersionProvider> _versionProviders;

        private static IEnumerable<IVersionProvider> GetDefaultVersionProviders()
        {
            yield break;
        }

        /// <summary>
        /// Gets the set of VersionProviders.
        /// </summary>
        public IEnumerable<IVersionProvider> VersionProviders
        {
            get => _versionProviders ?? (_versionProviders = GetDefaultVersionProviders());
            protected internal set => _versionProviders = (value ?? GetDefaultVersionProviders()).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionProviders"></param>
        protected BumpVersionServiceBase(params IVersionProvider[] versionProviders)
        {
            VersionProviders = versionProviders;
        }
    }
}
