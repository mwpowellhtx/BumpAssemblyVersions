using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
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
                var se = $@"(a-zA-Z\d{hyp})+";
                var xse = $@"({dot}{se})";
                var semantic = $"(?<semantic>{se}{xse}*)";
                return $@"\[assembly\: {attribName}\(""{version}({hyp}{semantic})?""\)\]";
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

        /// <summary>
        /// Returns whether the <paramref name="givenLines"/> Contains Any of the
        /// <paramref name="results"/> identified by the <see cref="AttributeRegexes"/> set of
        /// <see cref="Regex"/> pattern matchers.
        /// </summary>
        /// <param name="givenLines"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        protected bool ContainsAttribute(IEnumerable<string> givenLines, out IEnumerable<BumpMatch> results)
            => (results = givenLines.Select(
                        l => BumpMatch.Create(AttributeRegexes.FirstOrDefault(regex => regex.IsMatch(l))?.Match(l), l)
                    )
                ).Any(x => x.IsMatch);

        ///// <summary>
        ///// Returns whether the file on the other end of the <paramref name="assyInfoFullPath"/>
        ///// Contains any of the <paramref name="results"/>.
        ///// </summary>
        ///// <param name="assyInfoFullPath"></param>
        ///// <param name="results"></param>
        ///// <returns></returns>
        ///// <see cref="StringBumpVersionServiceBase{T}.ContainsAttribute"/>
        //private bool ContainsAttribute(string assyInfoFullPath, out IEnumerable<BumpMatch> results)
        //    => ContainsAttribute(ReadLinesFromFile(assyInfoFullPath), out results);

        /// <inheritdoc />
        public bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines)
        {
            resultLines = givenLines.ToArray();
            // TODO: TBD: examine the lines, may inject the Reflection using statement, bump the version, etc...
            return false;
        }
    }
}
