using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class UsingStatementAddedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string UsingStatement { get; }

        internal UsingStatementAddedEventArgs(string usingStatement)
        {
            UsingStatement = usingStatement;
        }
    }
}
