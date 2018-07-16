using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Represents unit <see cref="IStreamBumpVersionService.AttributeRegexes"/> tests connected
    /// with <see cref="AssemblyFileVersionAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyInformationalVersionStreamBumpVersionServiceAttributeRegexTests
        : BumpVersionServiceAttributeRegexTests<AssemblyInformationalVersionAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyInformationalVersionStreamBumpVersionServiceAttributeRegexTests(
            ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
