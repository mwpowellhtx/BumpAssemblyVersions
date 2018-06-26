using System;

namespace Bav
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class BuildResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Result.
        /// </summary>
        public BuildResult Result { get; }

        internal BuildResultEventArgs(BuildResult result)
        {
            Result = result;
        }
    }
}
