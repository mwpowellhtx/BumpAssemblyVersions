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
        /// 
        /// </summary>
        public ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// 
        /// </summary>
        public LoggerVerbosity Verbosity { get; set; } = Normal;

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected MSBuildInvocationServiceFactory(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <returns></returns>
        public abstract T GetService(ITestOutputHelper outputHelper);
    }
}
