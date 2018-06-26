using System;

namespace Bav
{
    /// <summary>
    /// This Provider is given as a kind of placeholder when instructions are otherwise void.
    /// </summary>
    /// <inheritdoc />
    public class UnknownVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets whether ForInternalUseOnly.
        /// </summary>
        /// <inheritdoc />
        public override bool ForInternalUseOnly { get; } = true;

        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Unknown";

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => throw new NotImplementedException();

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private UnknownVersionProvider(UnknownVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal UnknownVersionProvider()
        {
        }
    }
}
