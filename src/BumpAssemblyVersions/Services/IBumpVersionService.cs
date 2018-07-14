using System.Collections.Generic;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBumpVersionService
    {
        /// <summary>
        /// Gets the Mode.
        /// </summary>
        ServiceMode Mode { get; }

        /// <summary>
        /// Gets the set of VersionProviders.
        /// </summary>
        IEnumerable<IVersionProvider> VersionProviders { get; }
    }
}
