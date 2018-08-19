using System;
using System.Collections.Generic;

namespace Bav
{
    using Microsoft.Build.Utilities;

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

        /// <summary>
        /// Gets the Descriptor.
        /// </summary>
        IBumpVersionDescriptor Descriptor { get; }

        /// <summary>
        /// Gets or sets the Log.
        /// </summary>
        TaskLoggingHelper Log { get; set; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<BumpResultEventArgs> BumpResultFound;
    }
}
