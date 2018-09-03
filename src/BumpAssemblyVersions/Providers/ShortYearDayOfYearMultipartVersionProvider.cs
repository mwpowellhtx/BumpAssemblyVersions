using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    /// <summary>
    /// Changes the Version element to the concatenated values based on the Short
    /// <see cref="DateTime.Year"/> followed by the <see cref="DateTime.DayOfYear"/>.
    /// </summary>
    /// <inheritdoc />
    public class ShortYearDayOfYearMultipartVersionProvider : MultipartTimestampVersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Short Year Day Of Year";

        private IEnumerable<MultipartTimestampCallback> _callbacks;

        private static IEnumerable<MultipartTimestampCallback> GetMultipartCallbacks()
        {
            const int hundred = 100;

            yield return timestamp => $"{timestamp.Year % hundred:D02}";
            yield return timestamp => $"{timestamp.DayOfYear:D03}";
        }

        /// <summary>
        /// Gets the Callbacks involved for this Mutlipart Version Provider. This one
        /// involves <see cref="DateTime.Year"/> modulo one hundred for Short Year, and
        /// <see cref="DateTime.DayOfYear"/>.
        /// </summary>
        /// <inheritdoc />
        protected override IEnumerable<MultipartTimestampCallback> MultipartCallbacks
            => _callbacks ?? (_callbacks = GetMultipartCallbacks().ToArray());

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private ShortYearDayOfYearMultipartVersionProvider(ShortYearDayOfYearMultipartVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal ShortYearDayOfYearMultipartVersionProvider()
        {
        }
    }
}
