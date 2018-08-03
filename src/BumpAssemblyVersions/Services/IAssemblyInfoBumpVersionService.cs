using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bav
{
    using Microsoft.Build.Utilities;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc cref="IBumpVersionService" />
    public interface IAssemblyInfoBumpVersionService : IBumpVersionService
    {
        /// <summary>
        /// Gets the AttributeRegexes concerning the Service.
        /// </summary>
        IEnumerable<Regex> AttributeRegexes { get; }

        /// <summary>
        /// Tries to Bump the Version in the <paramref name="givenLines"/> resulting in the
        /// <paramref name="resultLines"/>.
        /// </summary>
        /// <param name="givenLines"></param>
        /// <param name="resultLines"></param>
        /// <returns></returns>
        bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines);

        /// <summary>
        /// Gets or sets the Log.
        /// </summary>
        TaskLoggingHelper Log { get; set; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<UsingStatementAddedEventArgs> UsingStatementAdded;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<BumpResultEventArgs> BumpResultFound;
    }
}
