using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBumpResultCalculator : IDisposable
    {
        /// <summary>
        /// Gets the Descriptor.
        /// </summary>
        IBumpVersionDescriptor Descriptor { get; }

        /// <summary>
        /// Tries to Bump the <paramref name="result"/>.
        /// </summary>
        /// <param name="given"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryBumpResult(string given, IBumpResult result);
    }
}
