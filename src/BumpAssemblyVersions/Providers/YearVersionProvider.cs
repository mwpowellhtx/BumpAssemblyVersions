using System;

namespace Bav
{
    /// <summary>
    /// Changes the Version element to the <see cref="DateTime.Year"/>.
    /// </summary>
    /// <inheritdoc />
    public class YearVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Year";

        private int GetYear() => ProtectedTimestamp.Year;

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = $"{GetYear()}") != current;

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private YearVersionProvider(YearVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal YearVersionProvider()
        {
        }
    }
}
