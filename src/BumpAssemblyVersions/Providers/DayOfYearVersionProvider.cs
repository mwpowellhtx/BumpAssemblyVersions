using System;

namespace Bav
{
    /// <summary>
    /// Changes the Version element to the <see cref="DateTime.DayOfYear"/>.
    /// </summary>
    /// <inheritdoc />
    public class DayOfYearVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Day Of Year";

        /// <summary>
        /// Performs the Provider Change strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{ProtectedTimestamp.DayOfYear}") != current;

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private DayOfYearVersionProvider(DayOfYearVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal DayOfYearVersionProvider()
        {
        }
    }
}
