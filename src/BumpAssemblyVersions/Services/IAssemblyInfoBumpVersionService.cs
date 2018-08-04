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
        /// 
        /// </summary>
        event EventHandler<UsingStatementAddedEventArgs> UsingStatementAdded;
    }
}
