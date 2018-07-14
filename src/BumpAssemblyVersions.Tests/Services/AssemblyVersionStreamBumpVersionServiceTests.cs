using System;
using System.Reflection;

namespace Bav
{
    using Xunit.Abstractions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class AssemblyVersionStreamBumpVersionServiceTests
        : StreamBumpVersionServiceTests<AssemblyVersionAttribute>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public AssemblyVersionStreamBumpVersionServiceTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
