using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    internal class ChangedVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Changed Provider";

        private const bool DefaultChanged = true;

        /// <summary>
        /// Public Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal ChangedVersionProvider()
        {
            Changed = DefaultChanged;
        }

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private ChangedVersionProvider(ChangedVersionProvider other)
            : base(other)
        {
            Changed = DefaultChanged;
        }

        /// <summary>
        /// Tries to Change the <paramref name="current"/> to the <paramref name="result"/>. This
        /// method is actually not implemented. Instead we throw
        /// <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
        {
            throw new NotImplementedException();
        }
    }
}
