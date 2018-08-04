using System;
using System.Collections.Generic;

namespace Bav
{
    using Microsoft.Build.Utilities;
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public abstract class BumpVersionServiceBase : IBumpVersionService
    {
        /// <inheritdoc />
        public ServiceMode Mode { get; protected internal set; } = VersionElements;

        /// <inheritdoc />
        public IBumpVersionDescriptor Descriptor { get; }

        public TaskLoggingHelper Log { get; set; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="descriptor"></param>
        protected BumpVersionServiceBase(IBumpVersionDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <inheritdoc />
        public event EventHandler<BumpResultEventArgs> BumpResultFound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        protected void OnBumpResultFound(BumpResult result)
            => BumpResultFound?.Invoke(this, new BumpResultEventArgs(result));

        /// <inheritdoc />
        public abstract bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines);

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
