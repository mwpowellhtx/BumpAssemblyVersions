using System;

namespace Bav
{
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class ConfigureEnvironmentVariablesEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public Toolset Toolset { get; }

        /// <summary>
        /// 
        /// </summary>
        public string InstallDirectoryName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolset"></param>
        /// <param name="installDirectoryName"></param>
        /// <inheritdoc />
        internal ConfigureEnvironmentVariablesEventArgs(Toolset toolset, string installDirectoryName)
        {
            Toolset = toolset;
            InstallDirectoryName = installDirectoryName;
        }
    }
}
