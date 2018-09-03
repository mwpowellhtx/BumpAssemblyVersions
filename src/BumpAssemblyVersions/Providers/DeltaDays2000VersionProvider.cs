using System;

namespace Bav
{
    /// <inheritdoc />
    public class DeltaDays2000VersionProvider : DeltaDaysVersionProviderBase
    {
        private static DateTime GetBaseTimestamp() => new DateTime(2000, 1, 1);

        /// <summary>
        /// Gets the BaseTimestamp.
        /// </summary>
        /// <inheritdoc />
        protected override DateTime BaseTimestamp { get; } = GetBaseTimestamp();

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private DeltaDays2000VersionProvider(DeltaDays2000VersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal DeltaDays2000VersionProvider()
        {
        }
    }
}
