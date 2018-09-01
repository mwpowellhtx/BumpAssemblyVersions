using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class ConfigureBuildEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string ProjectOrSolutionFullPath { get; set; }

        /// <summary>
        /// Gets or sets any Loggers you would like to involve.
        /// </summary>
        public IEnumerable<ILogger> Loggers { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, string> GlobalProperties { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> TargetsToBuild { get; set; }

        private static IEnumerable<T> DefaultCollection<T>()
        {
            yield break;
        }

        internal ConfigureBuildEventArgs()
        {
            Loggers = DefaultCollection<ILogger>().ToArray();
            GlobalProperties = new Dictionary<string, string>();
            TargetsToBuild = DefaultCollection<string>().ToArray();
        }
    }
}
