using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
    using static RegexOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    internal abstract class AssemblyInfoBumpVersionService : BumpVersionServiceBase
    {
        /// <summary>
        /// Returns the <see cref="ConstrainGenericTypesAttribute.AllowedTypes"/> specified
        /// against the <see cref="AssemblyInfoBumpVersionService{T}"/> generic
        /// <see cref="Type"/>. Note the subtle difference in angle brackets usage. We do not
        /// want the type specification in this instance.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetSupportedAttributeTypes()
            => typeof(AssemblyInfoBumpVersionService<>).GetGenericArguments()
                .First().GetCustomAttributes<ConstrainGenericTypesAttribute>()
                .SelectMany(constraint => constraint.AllowedTypes);

        /// <summary>
        /// Gets the set of SupportedAttributeTypes based on the
        /// <see cref="ConstrainGenericTypesAttribute"/> decoration.
        /// </summary>
        protected internal static IEnumerable<Type> SupportedAttributeTypes { get; }
            = GetSupportedAttributeTypes().ToArray();

        protected AssemblyInfoBumpVersionService(IBumpVersionDescriptor descriptor)
            : base(descriptor)
        {
        }
    }

    internal class AssemblyInfoBumpVersionService<[
                ConstrainGenericTypes(typeof(AssemblyVersionAttribute)
                    , typeof(AssemblyFileVersionAttribute)
                    , typeof(AssemblyInformationalVersionAttribute))]
            T>
        : AssemblyInfoBumpVersionService, IAssemblyInfoBumpVersionService
        where T : Attribute
    {
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
                var p = BumpResultCalculator.GetVersionRegexPattern();
                return $@"^\[assembly\: (?<attrib>{attribName})\(""{p}""\)\]$";
            }

            // There may be instances where it does not quite match the pattern.
            var names = new[] {AttributeType.ToShortName(), AttributeType.ToLongName()}.Distinct().ToArray();

            return names.Select(name => new Regex(GetRegexPattern(name), Compiled));
        }

        /// <inheritdoc />
        public IEnumerable<Regex> AttributeRegexes { get; } = GetAttributeRegexes().ToArray();

        /// <summary>
        /// Handshake the <typeparamref name="T"/> parameter using
        /// <see cref="AssemblyInfoBumpVersionService.SupportedAttributeTypes"/>
        /// as a frame of reference.
        /// </summary>
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

        public virtual bool TryBumpVersion(string given, out string result)
        {
            const char cr = '\r';
            const char lf = '\n';
            var crlf = $"{cr}{lf}";
            var givenLines = given.Replace(crlf, $"{lf}").Split(lf).Select(s => s ?? Empty).ToArray();
            var tried = TryBumpVersion(givenLines, out var resultLines);
            result = Join(crlf, (tried ? resultLines : givenLines).ToArray());
            // Tried, or Given changed from Result, sans any Trimmed Whitespace.
            return tried || given.Trim() != result.Trim();
        }

        public virtual bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines)
        {
            /* TODO: TBD: this whole regex-based approach is kind of a poor man's way of approaching
             the issue, injecting lines "manually", etc. I'm not sure it would be worth involving
             some sort of "diagnostic" or at least a Roslyn code analysis around the issue so that
             at least syntactic elements are specified in a prescribed manner. Will reserve that
             consideration for future work. */

            bool TryBumpGivenLine(string line, out AssemblyInfoBumpResult result)
            {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                Log?.LogWarning($"Trying to bump given line '{line}'.");
#endif

                result = new AssemblyInfoBumpResult(Descriptor) {AttributeType = AttributeType, Line = line};

                var match = AttributeRegexes.Select(regex => regex.Match(line)).SingleOrDefault(m => m.Success);

                if (match == null)
                {
                    return result.DidBump;
                }

                string overall;

                if (!(match.Groups.HasGroupName(nameof(overall))
                      || match.Groups[nameof(overall)].Success))
                {
                    return result.DidBump;
                }

                // TODO: TBD: once I have the overall match, then pass this along to the base level calculator
                // TODO: TBD: eventually, I think, either the Regex overall match based is a calculator
                // TODO: TBD: additionally, an XDocument based calculator
                overall = match.Groups[nameof(overall)].Value;

                // ReSharper disable once LocalNameCapturedOnly
                string attrib;

                if (!(match.Groups.HasGroupName(nameof(attrib))
                      || match.Groups[nameof(attrib)].Success))
                {
                    return result.DidBump;
                }

                // ReSharper disable once RedundantAssignment
                result.AttributeName = attrib = match.Groups[nameof(attrib)].Value;

                using (var calculator = new BumpResultCalculator(Descriptor))
                {
                    if (calculator.TryBumpResult(overall, result) && result.DidBump)
                    {
                        OnBumpResultFound(result);
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
