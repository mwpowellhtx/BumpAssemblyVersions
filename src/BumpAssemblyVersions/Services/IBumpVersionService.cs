using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public interface IBumpVersionService : IDisposable
    {
        /// <summary>
        /// Gets the Mode.
        /// </summary>
        ServiceMode Mode { get; }
    }
}
