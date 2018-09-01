﻿using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Represents unit <see cref="IAssemblyInfoBumpVersionService.AttributeRegexes"/> tests
    /// connected with <see cref="AssemblyFileVersionAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyInformationalVersionAssemblyInfoBumpVersionServiceTests
        : AssemblyInfoBumpVersionServiceTests<AssemblyInformationalVersionAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyInformationalVersionAssemblyInfoBumpVersionServiceTests(
            ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
