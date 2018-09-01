using System;

namespace Bav
{
    /// <inheritdoc />
    public class BumpResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Result.
        /// </summary>
        public IBumpResult Result { get; }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="result"></param>
        /// <inheritdoc />
        internal BumpResultEventArgs(IBumpResult result)
        {
            Result = result;
        }
    }
}
