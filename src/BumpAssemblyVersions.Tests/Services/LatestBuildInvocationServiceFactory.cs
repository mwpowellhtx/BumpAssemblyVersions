namespace Bav
{
    using Xunit.Abstractions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class LatestBuildInvocationServiceFactory
        : MSBuildInvocationServiceFactory<LatestBuildInvocationService>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override LatestBuildInvocationService GetService(ITestOutputHelper outputHelper)
            => new LatestBuildInvocationService(outputHelper, Verbosity);
    }
}
