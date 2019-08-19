using System;

namespace Bav
{
    using static Int16;

    /// <summary>
    /// Changes the Version element based on the number of days elapsed since
    /// <see cref="BaseTimestamp"/>.
    /// </summary>
    /// <inheritdoc />
    public abstract class DeltaDaysVersionProviderBase : VersionProviderBase
    {
        /// <summary>
        /// Override in order to provide the BaseTimestamp.
        /// </summary>
        protected abstract DateTime BaseTimestamp { get; }

        /// <summary>
        /// Gets the Actual Days involved during the Change attempt.
        /// </summary>
        protected internal int ActualDaysValue => (ProtectedTimestamp - BaseTimestamp).Days + 1;

        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <see cref="ActualDaysValue"/>
        /// <inheritdoc />
        public sealed override string Name => $"{ActualDaysValue} Days Since {BaseTimestamp:O}";

        /// <summary>
        /// Performs the Provider Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <see cref="ActualDaysValue"/>
        /// <inheritdoc />
        public sealed override bool TryChange(string current, out string result) => Changed = (result = $"{ActualDaysValue % MaxValue}") != current;

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Protected Default Constructor.
        /// </summary>
        /// <inheritdoc />
        protected DeltaDaysVersionProviderBase()
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        /// Protected Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        protected DeltaDaysVersionProviderBase(DeltaDaysVersionProviderBase other)
            : base(other)
        {
        }
    }
}
