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

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private SecondsSinceMidnightVersionProvider(SecondsSinceMidnightVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal SecondsSinceMidnightVersionProvider()
        {
        }
    }
}
