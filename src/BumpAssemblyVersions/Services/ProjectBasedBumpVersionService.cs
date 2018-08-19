using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bav
{
    using static String;
    using static XNode;
    using static SaveOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc cref="BumpVersionServiceBase" />
    /// <inheritdoc cref="IProjectBasedBumpVersionService" />
    public class ProjectBasedBumpVersionService : BumpVersionServiceBase, IProjectBasedBumpVersionService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <inheritdoc />
        public ProjectBasedBumpVersionService(IBumpVersionDescriptor descriptor)
            : base(descriptor)
        {
        }

        private static bool TryParseDocument(string given, out XDocument doc)
        {
            doc = XDocument.Parse(given);
            return true;

        }

        private static bool IsProjectDocument(XDocument doc, Func<XElement, bool> verifyRoot)
        {
            var root = doc?.Root;
            return root != null && verifyRoot(root);
        }

        private static bool TryBuildResult<T>(T node, out string result, SaveOptions options = None)
            where T : XNode
        {
            result = node.ToString(options);
            return !IsNullOrEmpty(result);
        }

        //// TODO: TBD: instead of string-based, we are approaching this one XDocument based...
        ///// <inheritdoc />
        //public override bool TryBumpVersion(string given, out string result)
        //{
        //    // ReSharper disable once InconsistentNaming
        //    const string Sdk = "Microsoft.NET.Sdk";
        //    return (TryParseDocument(given, out var doc)
        //            && IsProjectDocument(doc, root => root?.Attribute(nameof(Sdk))?.Value == Sdk)
        //            && (TryGetVersionElement(doc, out var versionElement, Descriptor)
        //                || TryCreateNewVersionElement(doc, out versionElement, Descriptor))
        //            && TryBumpVersionElement(versionElement, Descriptor)
        //            // TODO: TBD: this is where it gets really interesting...
        //            // TODO: TBD: methinks that the local function bits I introduced in the AssemblyInfo oriented Bumps may well best serve as base class protected members
        //            // TODO: TBD: then methinks that it should be fairly simple to invoke the same here, agnostic of the framework scaffolding...
        //            // TODO: TBD: i.e. whether the request originated in the AssemblyInfo or in the Xml Project file.
        //            // && TryBumpVersion ...
        //            && TryBuildResult(doc, out result))
        //           || (result = given) != given;
        //}

        /// <see cref="EqualityComparer"/>
        private static XNodeEqualityComparer Comparer => EqualityComparer;

        /// <inheritdoc />
        public virtual bool TryBumpDocument(XDocument given, out XDocument result)
        {
            var descriptor = Descriptor;

            // ReSharper disable once InconsistentNaming
            const string PropertyGroup = nameof(PropertyGroup);

            // ReSharper disable once ImplicitlyCapturedClosure
            bool TryGetVersionElement<TContainer>(TContainer container, out IEnumerable<XElement> elements)
                where TContainer : XContainer
            {
                var kind = $"{descriptor.Kind}";

                // TODO: TBD: actually, there could potentially be many of them, depending on the usage, filtering due to Configuration, etc...
                elements = container.Descendants(PropertyGroup).SelectMany(
                    propertyGroup => propertyGroup.Descendants(kind)).ToArray();
                return elements.Any();
            }

            // ReSharper disable once ImplicitlyCapturedClosure
            bool TryCreateNewVersionElement(XDocument doc, out IEnumerable<XElement> elements)
            {
                elements = new XElement[0];
                if (!descriptor.CreateNew)
                {
                    return false;
                }

                doc?.Root?.AddFirst(new XElement(PropertyGroup
                    , new XElement($"{descriptor.Kind}", descriptor.DefaultVersion))
                );

                return TryGetVersionElement(doc, out elements);
            }

            bool TryBumpVersionElements(params XElement[] elements)
            {
                // TODO: TBD: there could be potentially many of these, depending on filtering due to Configuration, etc
                // TODO: TBD: the "default" is a simple one for "all" Configurations, set one time

                bool TryBumpVersionElement(XElement element)
                {
                    // We require a new Calculator instance for every VersionElement that should be Bumped.
                    using (var calculator = new BumpResultCalculator(descriptor))
                    {
                        // TODO: TBD: this thing for now... we may want to make this be Configuration aware...
                        // TODO: TBD: this decision is TBD for a future time as I think it makes some sense to bump everything we find...
                        // TODO: TBD: that being said, I think Descriptor Provider specs will be Configuration based...
                        // TODO: TBD: So, we may need to pursue this avenue sooner than later...

                        var r = new ProjectBumpResult(Descriptor) {ProtectedElementName = element.Name.LocalName};

                        // TODO: TBD: could potentially condense this even further with a result-based callback
                        // TODO: TBD: through which we would do something with the VersionAndSemanticString

                        // ReSharper disable once InvertIf
                        if (calculator.TryBumpResult(element.Value, r) && r.DidBump)
                        {
                            OnBumpResultFound(r);
                            element.Value = r.VersionAndSemanticString;
                        }

                        return r.DidBump;
                    }
                }

                // TODO: TBD: may be able to optimize the ToArray bits down to simply .Count(TryBumpVersionElement)
                // There should be at least one Element and at least some (or one) that Bumped.
                return elements.Any() && elements.Select(TryBumpVersionElement).ToArray().Count(x => x) > 0;
            }

            bool DidBumpDocumentVersions<TNode>(TNode aDoc, TNode bDoc)
                where TNode : XNode
                => !Comparer.Equals(aDoc, bDoc);

            return (TryGetVersionElement(result = new XDocument(given), out var versionElements)
                    || TryCreateNewVersionElement(result, out versionElements))
                   && TryBumpVersionElements(versionElements.ToArray())
                   // TODO: TBD: this is where it gets really interesting...
                   // TODO: TBD: methinks that the local function bits I introduced in the AssemblyInfo oriented Bumps may well best serve as base class protected members
                   // TODO: TBD: then methinks that it should be fairly simple to invoke the same here, agnostic of the framework scaffolding...
                   // TODO: TBD: i.e. whether the request originated in the AssemblyInfo or in the Xml Project file.
                   // && TryBumpVersion ...
                   && DidBumpDocumentVersions(given, result);
        }
    }
}
