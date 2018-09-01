using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bav
{
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
        /// Tries to Bump the <paramref name="given"/> textual content from the Source, which
        /// may <paramref name="result"/> afterwards.
        /// </summary>
        /// <param name="given"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryBumpVersion(string given, out string result);

        /// <summary>
        /// Tries to Bump the Version in the <paramref name="givenLines"/> resulting in the
        /// <paramref name="resultLines"/>.
        /// </summary>
        /// <param name="givenLines"></param>
        /// <param name="resultLines"></param>
        /// <returns></returns>
        bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines);

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<UsingStatementAddedEventArgs> UsingStatementAdded;
    }
}
