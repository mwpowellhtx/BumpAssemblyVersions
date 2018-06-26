using System;

namespace Bav
{
    /// <summary>
    /// Changes the Version element based on <see cref="DateTime.Month"/>.
    /// </summary>
    /// <inheritdoc />
    public class MonthVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Month";

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{ProtectedTimestamp.Month}") != current;

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private MonthVersionProvider(MonthVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal MonthVersionProvider()
        {
        }
    }
}
