using System;

namespace Bav
{
    /// <inheritdoc />
    public class DeltaDays1980VersionProvider : DeltaDaysVersionProviderBase
    {
        private static DateTime GetBaseTimestamp() => new DateTime(1980, 1, 1);

        /// <summary>
        /// Gets the BaseTimestmap.
        /// </summary>
        /// <inheritdoc />
        protected override DateTime BaseTimestamp { get; } = GetBaseTimestamp();

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private DeltaDays1980VersionProvider(DeltaDays1980VersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal DeltaDays1980VersionProvider()
        {
        }
    }
}
