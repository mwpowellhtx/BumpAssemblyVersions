using System;

namespace Bav
{
    /// <inheritdoc />
    public class DeltaDays1970VersionProvider : DeltaDaysVersionProviderBase
    {
        private static DateTime GetBaseTimestamp() => new DateTime(1970, 1, 1);

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
        private DeltaDays1970VersionProvider(DeltaDays1970VersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal DeltaDays1970VersionProvider()
        {
        }
    }
}
