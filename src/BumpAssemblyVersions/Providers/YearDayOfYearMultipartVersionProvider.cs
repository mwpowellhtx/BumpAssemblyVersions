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
    public class YearDayOfYearMultipartVersionProvider : MultipartTimestampVersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Year Day Of Year";

        private IEnumerable<MultipartTimestampCallback> _callbacks;

        private static IEnumerable<MultipartTimestampCallback> GetMultipartCallbacks()
        {
            yield return timestamp => $"{timestamp.Year}";
            yield return timestamp => $"{timestamp.DayOfYear:D03}";
        }

        /// <summary>
        /// Specifies callbacks involving <see cref="DateTime.Year"/> and
        /// <see cref="DateTime.DayOfYear"/>.
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
        private YearDayOfYearMultipartVersionProvider(YearDayOfYearMultipartVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal YearDayOfYearMultipartVersionProvider()
        {
        }
    }
}
