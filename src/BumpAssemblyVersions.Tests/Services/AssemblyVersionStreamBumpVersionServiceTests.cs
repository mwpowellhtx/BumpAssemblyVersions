using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Represents unit <see cref="IStreamBumpVersionService.AttributeRegexes"/> tests connected
    /// with <see cref="AssemblyVersionAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyVersionStreamBumpVersionServiceAttributeRegexTests
        : BumpVersionServiceAttributeRegexTests<AssemblyVersionAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyVersionStreamBumpVersionServiceAttributeRegexTests(
            ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
