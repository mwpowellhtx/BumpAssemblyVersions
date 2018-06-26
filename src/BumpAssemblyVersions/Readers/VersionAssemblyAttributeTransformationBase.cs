using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using static String;
    using static StringSplitOptions;

    internal abstract class VersionAssemblyAttributeTransformationBase<TAttribute>
        : AssemblyAttributeTransformationBase<TAttribute, Version>
        where TAttribute : Attribute
    {
        protected VersionAssemblyAttributeTransformationBase(string sourcePath)
            : base(sourcePath)
        {
        }

        protected override Version Parse(string s) => Version.Parse(s);

        // TODO: TBD: then we need to connect the dots: should have building project path in hand, look for the CS files having the attribute
        // TODO: TBD: arrange for the providers, either via "descriptors", or the "providers" ARE the descriptors, 
        public override bool TryChangeVersioning(params IVersionProvider[] providers)
        {
            if (!base.TryChangeVersioning(providers))
            {
                return false;
            }

            // Ensures that we can cover any unspecified gaps in trailing elements.
            IEnumerable<string> NormalizeElements(IEnumerable<string> x)
            {
                // Take as many as FOUR, but IGNORE any other ones.
                const int maxElementsLength = 4;
                var i = 0;
                return x.TakeWhile(_ => i++ < maxElementsLength).ToArray();
            }

            // As well as aligning the set of corresponding Providers.
            IEnumerable<IVersionProvider> NormalizeProviders(IVersionProvider[] p, IReadOnlyCollection<string> s)
            {
                while (p.Length < s.Count)
                {
                    p = p.Concat(new IVersionProvider[] {new NoOpVersionProvider()}).ToArray();
                }

                return p;
            }

            const char wildcard = '*';
            const char dot = '.';
            // TODO: TBD: must account for wildcard (non-deterministic?): i.e. "*"
            var versionBeforeString = Match.Value;
            //var versionBefore = Parse(versionBeforeString);
            var hasWildcard = versionBeforeString.Contains(wildcard);
            // Which we can utilize the Wildcard as a delimiter as well and just Remove the Empty Entries.
            var elements = NormalizeElements(versionBeforeString.Split(new[] {dot, wildcard}, RemoveEmptyEntries)
                .Select(element => element ?? Empty)).ToArray();

            providers = NormalizeProviders(providers, elements).ToArray();

            // TODO: TBD: if it weren't for the fact that we are potentially dealing with different animals in the input, mid-stream processing, and output, etc...
            var zipped = providers.Take(elements.Length).Zip(elements, (provider, elementBefore) =>
            {
                var changed = provider.TryChange(elementBefore, out var elementAfter);
                return new {Changed = changed, ElementBefore = elementBefore, ElementAfter = elementAfter};
            }).ToArray();

            // Short circuit the whole mess when nothing changed.
            if (!zipped.Any(x => x.Changed))
            {
                return false;
            }

            // Roll up the Elements, including possibly the Wildcard Element.
            var elementsAfter = zipped.Select(x => x.ElementAfter).ToArray();

            // Build a Version String involving the Wildcard element.
            string BuildWildcardVersionAfterString(params string[] s)
                => Join($"{dot}", s.Concat(new[] {$"{wildcard}"}));

            // Note the difference, we actually Parse a Version when there is No Wildcard.
            Version BuildVersionAfter(params string[] s)
                => Parse(Join($"{dot}", elementsAfter));

            var versionAfterString = hasWildcard
                ? BuildWildcardVersionAfterString(elementsAfter)
                : $"{BuildVersionAfter(elementsAfter)}";

            // Short circuit additionally when the string itself has not actually changed.
            if (versionAfterString == versionBeforeString)
            {
                return false;
            }

            ReplaceAttributeUsage(() => $"[assembly: {ShortAttributeName}(\"{versionAfterString}\")]");

            return true;
        }
    }
}
