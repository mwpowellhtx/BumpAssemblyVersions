using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    /// <summary>
    /// Changes the Version element based on <see cref="DateTime.Month"/> and
    /// <see cref="DateTime.Day"/>.
    /// </summary>
    /// <inheritdoc />
    public class MonthDayOfMonthMultipartVersionProvider : MultipartTimestampVersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Month Day Of Month";

        private IEnumerable<MultipartTimestampCallback> _callbacks;

        private static IEnumerable<MultipartTimestampCallback> GetMultipartCallbacks()
        {
            yield return timestamp => $"{timestamp.Month:D02}";
            yield return timestamp => $"{timestamp.Day:D02}";
        }

        /// <summary>
        /// Gets the Callbacks involved for this Multipart Version Provider. This one
        /// involves <see cref="DateTime.Month"/> and <see cref="DateTime.Day"/>.
        /// </summary>
        /// <inheritdoc />
        protected override IEnumerable<MultipartTimestampCallback> MultipartCallbacks
            => _callbacks ?? (_callbacks = GetMultipartCallbacks().ToArray());

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private MonthDayOfMonthMultipartVersionProvider(MonthDayOfMonthMultipartVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal MonthDayOfMonthMultipartVersionProvider()
        {
        }
    }
}
