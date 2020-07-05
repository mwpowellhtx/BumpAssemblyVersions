using System;

namespace Bav
{
    /// <summary>
    /// We do not care what this particular concrete Version Provider actually might have done.
    /// Only that it can be observered to have <see cref="VersionProviderBase.Changed"/>. This is
    /// for unit testing purposes only and adds no real value in terms of <see cref="TryChange"/>
    /// behavior.
    /// </summary>
    /// <inheritdoc />
    public class ChangedVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Changed Provider";

        private const bool DefaultChanged = true;

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal ChangedVersionProvider()
            : this((bool?) null)
        {
        }

        /// <summary>
        /// Internal Changed Constructor.
        /// </summary>
        /// <param name="changed"></param>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal ChangedVersionProvider(bool? changed = null)
        {
            Changed = changed ?? DefaultChanged;
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
