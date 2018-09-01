using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Represents unit <see cref="IAssemblyInfoBumpVersionService.AttributeRegexes"/> tests
    /// connected with <see cref="AssemblyVersionAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyVersionAssemblyInfoBumpVersionServiceTests
        : AssemblyInfoBumpVersionServiceTests<AssemblyVersionAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyVersionAssemblyInfoBumpVersionServiceTests(
            ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
