namespace Bav
{
    using Xunit.Abstractions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class LatestBuildInvocationServiceFactory : MSBuildInvocationServiceFactory<LatestBuildInvocationService>
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public LatestBuildInvocationServiceFactory(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <returns></returns>
        public override LatestBuildInvocationService GetService(ITestOutputHelper outputHelper)
            => new LatestBuildInvocationService(outputHelper, Verbosity);
    }
}
