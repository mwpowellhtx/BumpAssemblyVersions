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
        /// Tries to Bump the Version in the <paramref name="givenLines"/> resulting in the
        /// <paramref name="resultLines"/>.
        /// </summary>
        /// <param name="givenLines"></param>
        /// <param name="resultLines"></param>
        /// <returns></returns>
        bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines);

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
