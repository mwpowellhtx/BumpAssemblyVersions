using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;

namespace Bav
{
    using static String;
    using static VersionProviderTemplateRegistry;
    using static RegexOptions;

    internal class AssemblyInfoBumpVersionService<[
                ConstrainGenericTypes(typeof(AssemblyVersionAttribute)
                    , typeof(AssemblyFileVersionAttribute)
                    , typeof(AssemblyInformationalVersionAttribute))]
            T>
        : BumpVersionServiceBase, IAssemblyInfoBumpVersionService
        where T : Attribute
    {
        /// <summary>
        /// Returns the <see cref="ConstrainGenericTypesAttribute.AllowedTypes"/> specified
        /// against the <see cref="Type"/> <typeparamref name="T"/>. Note the subtle difference
        /// in angle brackets usage. We do not want the type specification in this instance.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetSupportedAttributeTypes()
            => typeof(AssemblyInfoBumpVersionService<>).GetGenericArguments()
                .First().GetCustomAttributes<ConstrainGenericTypesAttribute>()
                .SelectMany(constraint => constraint.AllowedTypes);

        public TaskLoggingHelper Log { get; set; }

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        /// Gets the set of SupportedAttributeTypes based on the
        /// <see cref="ConstrainGenericTypesAttribute"/> decoration.
        /// </summary>
        protected internal static IEnumerable<Type> SupportedAttributeTypes { get; }
            = GetSupportedAttributeTypes().ToArray();

        /// <summary>
        /// 
        /// </summary>
        protected static readonly Type AttributeType = typeof(T);

        // TODO: TBD: there may be a base class AttributeRegexes that should be discovered...
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static IEnumerable<Regex> GetAttributeRegexes()
        {
            // We will return the naive pattern matching for faster identification.
            string GetRegexPattern(string attribName)
            {
                /*
                 * Went with an abbreviated notation here for brevity:
                 * wc = wildcard
                 * v = version
                 * s = semantic
                 * e = element
                 * x = extended, meaning dot delimited
                 */
                const string dot = @"\.";
                const string hyp = @"\-";
                var wc = $@"({dot}\*)";
                const string ve = @"(\d)+";
                var xve = $@"({dot}{ve})";
                var version = $@"(?<version>{ve}{xve}({wc}|{xve}{wc}?|{xve}{{2}})?)";
                // SE needs to be One Or More of Any of these Bits.
                var se = $@"[a-zA-Z\d{hyp}]+";
                var xse = $@"({dot}{se})";
                var semantic = $"(?<semantic>{se}{xse}*)";
                return $@"^\[assembly\: (?<attrib>{attribName})\(""{version}({hyp}{semantic})?""\)\]$";
            }

            // There may be instances where it does not quite match the pattern.
            var names = new[] {AttributeType.ToShortName(), AttributeType.ToLongName()}.Distinct().ToArray();

            return names.Select(name => new Regex(GetRegexPattern(name), Compiled));
        }

        /// <inheritdoc />
        public IEnumerable<Regex> AttributeRegexes { get; } = GetAttributeRegexes().ToArray();

        static AssemblyInfoBumpVersionService()
        {
            if (SupportedAttributeTypes.Any(type => type == AttributeType))
            {
                return;
            }

            var supported = Join(", ", SupportedAttributeTypes.Select(type => $"'{type.FullName}'"));

            throw new InvalidOperationException(
                $"'{AttributeType.FullName}' is not supported"
                + $" by '{typeof(AssemblyInfoBumpVersionService<T>)}': {supported}."
            );
        }

        internal AssemblyInfoBumpVersionService(IBumpVersionDescriptor bumpVersionDescriptor)
            : base(bumpVersionDescriptor)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <inheritdoc />
        public event EventHandler<UsingStatementAddedEventArgs> UsingStatementAdded;

        private void OnUsingStatementAdded(string usingStatement)
            => UsingStatementAdded?.Invoke(this
                , new UsingStatementAddedEventArgs(usingStatement));

        /// <summary>
        /// 
        /// </summary>
        /// <inheritdoc />
        public event EventHandler<BumpResultEventArgs> BumpResultFound;

        private void OnBumpResultFound(BumpResult result)
            => BumpResultFound?.Invoke(this, new BumpResultEventArgs(result));

        /// <inheritdoc />
        public bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines)
        {
            /* TODO: TBD: this whole regex-based approach is kind of a poor man's way of approaching
             the issue, injecting lines "manually", etc. I'm not sure it would be worth involving
             some sort of "diagnostic" or at least a Roslyn code analysis around the issue so that
             at least syntactic elements are specified in a prescribed manner. Will reserve that
             consideration for future work. */

            bool TryBumpGivenLine(string line, out BumpResult result)
            {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                Log?.LogWarning($"Trying to bump given line '{line}'.");
#endif

                result = new BumpResult {AttributeType = AttributeType, Line = line};

                var match = AttributeRegexes.Select(regex => regex.Match(line)).SingleOrDefault(m => m.Success);

                if (match == null)
                {
                    return result.DidBump;
                }

                // TODO: TBD: provide hooks whether to 1) Support Wildcard, and 2) Supports Pre-Release Semantic Version.
                const char dot = '.';
                const char wildcard = '*';

                // ReSharper disable once LocalNameCapturedOnly
                string attrib, version;

                if (!(match.Groups.HasGroupName(nameof(attrib))
                      || match.Groups.HasGroupName(nameof(version))
                      || match.Groups[nameof(attrib)].Success
                      || match.Groups[nameof(version)].Success))
                {
                    return result.DidBump;
                }

                // ReSharper disable once RedundantAssignment
                result.AttributeName = attrib = match.Groups[nameof(attrib)].Value;

                /* Capture this once and once only across the breadth of the request. Yes, we want
                 to capture a whole new collection, because any prior matches in the same file will
                 also need to have been bumped in a consistent manner, including any More Significant
                 Providers in prior sequences. */

                var versionProviders = Descriptor.VersionProviders.ToArray();

                // TODO: TBD: should "wildcard" be a special field? potentially, it could even be its own Version Provider, i.e. WildcardVersionProvider ...
                // TODO: TBD: which would be a long-handed way of saying "*" ... but it might make better sense than reading around the wildcard field ...
                var versionElements = (result.OldVersionString = match.Groups[nameof(version)].Value).Split(dot);
                var hasWildcard = versionElements.Any(x => x == $"{wildcard}");

                var changedElements = versionElements
                    .Take(versionElements.Length - (hasWildcard ? 1 : 0))
                    .Zip(versionProviders.Take(versionElements.Length - (hasWildcard ? 1 : 0))
                        , (x, p) => p.TryChange(x, out var y) ? y : x).ToArray();

                result.VersionString = version = Join($"{dot}", changedElements);

                if (hasWildcard)
                {
                    // ReSharper disable once RedundantAssignment
                    result.VersionString = version = Join($"{dot}", version, $"{wildcard}");
                }

                /* TODO: TBD: there is a corner case in here whereby we would not necessarily want a
                 Wildcard to coexist with a Semantic, but we will leave that go for the time being... */
                string semantic;

                // ReSharper disable once InvertIf
                // Having the Group does not necessarily mean Successful Match.
                if (match.Groups.HasGroupName(nameof(semantic))
                    && match.Groups[nameof(semantic)].Success)
                {
                    semantic = result.OldSemanticString = match.Groups[nameof(semantic)].Value;

                    /* Semantic simply falls through as-is when there is no Provider given.
                     However, not being given a Provider is effectively a NoOp. */
                    var p = versionProviders.OfType<PreReleaseIncrementVersionProvider>().SingleOrDefault()
                            ?? (IVersionProvider) Registry.NoOp;

                    if (p is PreReleaseIncrementVersionProvider preRelease
                        && preRelease.TryChange(semantic, out var newSemantic))
                    {
                        result.SemanticString = newSemantic;
                    }
                    else
                    {
                        p.TryChange(semantic, out var noOpSemantic);
                        result.SemanticString = noOpSemantic;
                    }
                }

                return result.DidBump;
            }

            IEnumerable<string> BumpGivenLines(out int bumpCount, params string[] lines)
            {
                bumpCount = 0;
                var bumpedLines = new List<string>();
                foreach (var line in lines)
                {
                    bumpedLines.Add(line);

                    if (!TryBumpGivenLine(line, out var result) || !result.DidBump)
                    {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                        Log?.LogWarning($"Did not bump line '{line}'.");
#endif

                        continue;
                    }

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    Log?.LogWarning($"Did bump line '{result.Result}'.");
#endif

                    OnBumpResultFound(result);

                    ++bumpCount;
                    bumpedLines[bumpedLines.Count - 1] = result.Result;
                }

                void AddUsingStatement()
                {
                    // Insert the Using Statement when there was not one before.
                    var usingStatement = $"using {AttributeType.Namespace};";

                    if (bumpedLines.Any(result => result == usingStatement))
                    {
                        return;
                    }

                    OnUsingStatementAdded(usingStatement);

                    bumpedLines.Insert(0, usingStatement);
                }

                // TODO: TBD: the functionality could potentially go on the interface itself ?
                string GetDefaultVersion(IBumpVersionDescriptor descriptor, string defaultDefaultVersion = null)
                    => descriptor.DefaultVersion ?? (defaultDefaultVersion ?? $"{new Version(0, 0, 0)}");

                if (bumpCount > 0)
                {
                    AddUsingStatement();
                }
                else if (bumpCount == 0
                         && Descriptor.CreateNew
                         && TryBumpGivenLine(
                             $"[assembly: {AttributeType.ToShortName()}"
                             + $"(\"{GetDefaultVersion(Descriptor)}\")]"
                             , out var result))
                {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    Log?.LogWarning($"Creating new version '{result.Result}'.");
#endif

                    AddUsingStatement();

                    OnBumpResultFound(result);

                    ++bumpCount;
                    bumpedLines.Add(result.Result);
                }

                return bumpedLines.ToArray();
            }

            // ReSharper disable once PossibleMultipleEnumeration
            resultLines = BumpGivenLines(out var resultCount, givenLines.ToArray()).ToArray();

            return resultCount > 0;
        }
    }
}
