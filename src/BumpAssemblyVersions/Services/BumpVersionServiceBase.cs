﻿namespace Bav
{
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public abstract class BumpVersionServiceBase : IBumpVersionService
    {
        /// <inheritdoc />
        public ServiceMode Mode { get; protected internal set; } = VersionElements;

        /// <summary>
        /// Indicates whether the Object IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the Object whether <paramref name="disposing"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Disposes the Object.
        /// </summary>
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
