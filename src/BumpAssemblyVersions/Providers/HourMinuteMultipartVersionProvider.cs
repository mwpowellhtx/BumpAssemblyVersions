using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    /// <summary>
    /// Changes the Version element based on the <see cref="DateTime.Hour"/> and
    /// <see cref="DateTime.Minute"/>.
    /// </summary>
    /// <inheritdoc />
    public class HourMinuteMultipartVersionProvider : MultipartTimestampVersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Hour Minute";

        private IEnumerable<MultipartTimestampCallback> _callbacks;

        private static IEnumerable<MultipartTimestampCallback> GetMultipartCallbacks()
        {
            yield return timestamp => $"{timestamp.Hour:D02}";
            yield return timestamp => $"{timestamp.Minute:D02}";
        }

        /// <summary>
        /// Gets the Callbacks involved with this Multipart Version Provider. This one
        /// involves <see cref="DateTime.Hour"/> and <see cref="DateTime.Minute"/>.
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
        private HourMinuteMultipartVersionProvider(HourMinuteMultipartVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal HourMinuteMultipartVersionProvider()
        {
        }
    }
}
