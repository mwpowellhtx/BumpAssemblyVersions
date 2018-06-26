using System;

namespace Bav
{
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class ToolsetRequiredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Preidcate.
        /// </summary>
        public Func<Toolset, bool> Predicate { get; set; }

        /// <summary>
        /// Gets or sets the Install Directory Name getter.
        /// </summary>
        public Func<Toolset, string> GetInstallDirectoryName { get; set; }

        internal ToolsetRequiredEventArgs()
        {
            Predicate = ts => throw new NotImplementedException();
            GetInstallDirectoryName = ts => throw new NotImplementedException();
        }
    }
}
