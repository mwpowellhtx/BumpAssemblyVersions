using System;

namespace Bav
{
    /// <inheritdoc />
    public class DeltaDays1900VersionProvider : DeltaDaysVersionProviderBase
    {
        private static DateTime GetBaseTimestamp() => new DateTime(1900, 1, 1);

        /// <summary>
        /// Gets the BaseTimestamp.
        /// </summary>
        /// <inheritdoc />
        protected override DateTime BaseTimestamp { get; } = GetBaseTimestamp();

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private DeltaDays1900VersionProvider(DeltaDays1900VersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        internal DeltaDays1900VersionProvider()
        {
        }
    }
}
