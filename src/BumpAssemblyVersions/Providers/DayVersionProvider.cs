using System;

namespace Bav
{
    /// <summary>
    /// Changes the Version element based on the <see cref="DateTime.Day"/>.
    /// </summary>
    /// <inheritdoc />
    public class DayVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Day Of Month";

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{ProtectedTimestamp.Day}") != current;

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private DayVersionProvider(DayVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal DayVersionProvider()
        {
        }
    }
}
