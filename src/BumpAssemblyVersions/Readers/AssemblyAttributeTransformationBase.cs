using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
    using static FileAccess;
    using static FileMode;
    using static RegexOptions;

    internal abstract class AssemblyAttributeTransformationBase<TAttribute, TVersion>
        where TAttribute : Attribute
        where TVersion : class
    {
        private readonly string _longAttributeName = typeof(TAttribute).Name;

        private string _shortAttributeName;

        protected string ShortAttributeName
            => _shortAttributeName ?? (_shortAttributeName = _longAttributeName
                   .Substring(0, _longAttributeName.Length - nameof(Attribute).Length));

        private Match _match;

        protected Match Match => _match;

        private bool? _hasAttribute;

        private bool HasAttribute
        {
            get
            {
                _hasAttribute = _hasAttribute ?? TryEvaluateSource(out _match);
                return _hasAttribute ?? false;
            }
        }

        internal string SourcePath { get; }

        private IEnumerable<Regex> _patterns;

        private IEnumerable<Regex> Patterns
        {
            get
            {
                IEnumerable<Regex> GetPatterns()
                {
                    const string assembly = nameof(assembly);

                    Regex GetPattern(string name)
                    {
                        // TODO: TBD: for now we will define it naively; however, we may want to decompose the Major, Minor, Build, Patch, etc...
                        return new Regex($"^\\[{assembly}\\:\\s{name}\\((?<s>[a-zA-Z\\d\\.\\-\\+\\*]+)\\)\\]$", Compiled);
                    }

                    yield return GetPattern(_longAttributeName);
                    yield return GetPattern(ShortAttributeName);
                }

                return _patterns ?? (_patterns = GetPatterns().ToArray());
            }
        }

        private bool TryEvaluateSource(out Match match)
        {
            match = null;

            // TODO: TBD: may hang onto the file in order to replace in-situ with the match...
            using (var fs = File.Open(SourcePath, Open, Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine() ?? Empty;

                        match = Patterns.SingleOrDefault(pattern => pattern.IsMatch(line))?.Match(line);
                    }
                }
            }

            return match?.Success ?? false;
        }

        protected abstract TVersion Parse(string s);

        public virtual bool TryChangeVersioning(params IVersionProvider[] providers)
        {
            if (!HasAttribute)
            {
                return false;
            }

            // TODO: TBD: this may be better arranged by the caller, some sort of "transformation context"...
            for (var i = 1; i < providers.Length; i++)
            {
                // Ensure that the Providers have Awareness of the Providers which supersede them.
                ((VersionProviderBase) providers[i]).MoreSignificantProviders = providers.Take(i).ToArray();
            }

            return true;
        }

        protected virtual void ReplaceAttributeUsage(Func<string> getNewLine)
        {
            var tempLines = new List<string>();

            // Open the file for Reading.
            using (var fs = File.Open(SourcePath, Open, Read, FileShare.Read))
            {
                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine() ?? Empty;

                        // Stripping out any CR+NL characters.
                        tempLines.Add(line.Replace("\r", Empty).Replace("\n", Empty));

                        // TODO: TBD: probably do not need to involve Regex at this level...
                        if (!line.Contains(ShortAttributeName))
                        {
                            continue;
                        }

                        tempLines[tempLines.Count - 1] = getNewLine();
                    }
                }
            }

            // Then re-Open the same file for re-Writing.
            using (var fs = File.Open(SourcePath, Create, Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(fs))
                {
                    // The Lines should all be arranged by this point.
                    tempLines.ForEach(writer.WriteLine);
                }
            }
        }

        protected AssemblyAttributeTransformationBase(string sourcePath)
        {
            SourcePath = sourcePath;
        }
    }
}
