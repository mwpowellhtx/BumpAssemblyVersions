using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    /// <summary>
    /// Represents unit <see cref="IStreamBumpVersionService.AttributeRegexes"/> tests connected
    /// with <see cref="AssemblyFileVersionAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyFileVersionStreamBumpVersionServiceAttributeRegexTests
        : BumpVersionServiceAttributeRegexTests<AssemblyFileVersionAttribute>
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyFileVersionStreamBumpVersionServiceAttributeRegexTests(
            ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
