using Microsoft.Build.Framework;

namespace Bav
{
    using Xunit.Abstractions;
    using static LoggerVerbosity;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MSBuildInvocationServiceFactory<T>
        where T : MSBuildInvocationService
    {
        /// <summary>
        /// Gets the OutputHelper.
        /// </summary>
        public ITestOutputHelper OutputHelper { get; internal set; }

        /// <summary>
        /// Gets the Verbosity.
        /// </summary>
        public LoggerVerbosity Verbosity { get; set; } = Normal;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <returns></returns>
        public abstract T GetService(ITestOutputHelper outputHelper);
    }
}
