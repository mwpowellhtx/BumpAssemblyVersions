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

        /// <summary>
        /// Gets the Exception that occurred, if any.
        /// </summary>
        public InvalidOperationException Exception { get; }

        internal BuildResultEventArgs(BuildResult result)
            : this(result, null)
        {
        }

        internal BuildResultEventArgs(BuildResult result, InvalidOperationException exception)
        {
            Result = result;
            Exception = exception;
        }
    }
}
