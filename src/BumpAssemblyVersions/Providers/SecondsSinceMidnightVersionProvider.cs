using System;

namespace Bav
{
    using static Int16;
    using static Math;

    /// <summary>
    /// Changes the Version element to the Total Seconds since Midnight.
    /// </summary>
    /// <inheritdoc />
    public class SecondsSinceMidnightVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Seconds Since Midnight";

        private int GetTotalSecondsSinceMidnight()
            => (int) Abs((ProtectedTimestamp - MidnightTimestamp).TotalSeconds) % MaxValue;

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{GetTotalSecondsSinceMidnight()}") != current;

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private SecondsSinceMidnightVersionProvider(SecondsSinceMidnightVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal SecondsSinceMidnightVersionProvider()
        {
        }
    }
}
