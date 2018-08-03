using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class BumpResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Result.
        /// </summary>
        public BumpResult Result { get; }

        internal BumpResultEventArgs(BumpResult result)
        {
            Result = result;
        }
    }
}
