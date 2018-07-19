using System.Text.RegularExpressions;

namespace Bav
{
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public abstract class BumpVersionServiceBase : IBumpVersionService
    {
        /// <inheritdoc />
        public ServiceMode Mode { get; protected internal set; } = VersionElements;

        // TODO: TBD: this one may need to be promoted to first class assembly wide entity...
        /// <summary>
        /// 
        /// </summary>
        protected class BumpMatch
        {
            internal string Line { get; }

            internal Match Match { get; }

            internal bool IsMatch => Match?.Success ?? false;

            private BumpMatch(Match match, string line)
            {
                Match = match;
                Line = line;
            }

            internal static BumpMatch Create(Match match, string line) => new BumpMatch(match, line);
        }

        /// <summary>
        /// Indicates whether the Object IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the Object whether <paramref name="disposing"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Disposes the Object.
        /// </summary>
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
