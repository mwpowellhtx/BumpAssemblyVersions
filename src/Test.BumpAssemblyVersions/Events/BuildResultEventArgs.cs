using System;

namespace Bav
{
    using Microsoft.Build.Execution;

    /// <inheritdoc />
    public class BuildResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or Sets whether IsHandled.
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Gets the Result.
        /// </summary>
        public BuildResult Result { get; }

        /// <summary>
        /// Gets the Exception that occurred, if any.
        /// </summary>
        public Exception Exception { get; }

        internal BuildResultEventArgs(BuildResult result)
            : this(result, null)
        {
        }

        internal BuildResultEventArgs(BuildResult result, Exception exception)
        {
            Result = result;
            Exception = exception;
        }
    }
}
