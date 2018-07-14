using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc cref="IBumpVersionService" />
    public interface IStreamBumpVersionService : IBumpVersionService, IDisposable
    {
        /// <summary>
        /// Gets the AttributeRegexes concerning the Service.
        /// </summary>
        IEnumerable<Regex> AttributeRegexes { get; }
    }
}
