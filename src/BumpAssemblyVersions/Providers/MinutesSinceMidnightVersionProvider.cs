using System;

namespace Bav
{
    using static Int16;
    using static Math;

    /// <summary>
    /// Changes the Version element to the Total Minutes since Midnight.
    /// </summary>
    /// <inheritdoc />
    public class MinutesSinceMidnightVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Minutes Since Midnight";

        private int GetTotalMinutesSinceMidnight()
            => (int) Abs((ProtectedTimestamp - MidnightTimestamp).TotalMinutes) % MaxValue;

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{GetTotalMinutesSinceMidnight()}") != current;

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private MinutesSinceMidnightVersionProvider(MinutesSinceMidnightVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal MinutesSinceMidnightVersionProvider()
        {
        }
    }
}
