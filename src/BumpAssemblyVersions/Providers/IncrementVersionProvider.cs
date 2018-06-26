using System;

namespace Bav
{
    using static Int32;

    /// <inheritdoc />
    public class IncrementVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Increment";

        private static int Increment(int current)
        {
            const short maxValue = short.MaxValue;
            var currentpp = current + 1;
            return currentpp > maxValue ? 0 : currentpp;
        }

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{(ShouldReset ? 0 : Increment(Parse(current)))}") != current;

        /// <summary>
        /// Protected Copy Constructor
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        /// <remarks>This Copy Constructor is Protected due to the fact that it will be
        /// utilized by at least one Class deriving from it.</remarks>
        // ReSharper disable once SuggestBaseTypeForParameter
        protected IncrementVersionProvider(IncrementVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal IncrementVersionProvider()
        {
        }
    }
}
