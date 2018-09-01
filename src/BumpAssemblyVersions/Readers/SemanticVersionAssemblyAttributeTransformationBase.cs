using System;

namespace Bav
{
    using NuGet.Versioning;

    internal abstract class SemanticVersionAssemblyAttributeTransformationBase<TAttribute, TVersion>
        : AssemblyAttributeTransformationBase<TAttribute, TVersion>
        where TAttribute : Attribute
        where TVersion : SemanticVersion
    {
        protected SemanticVersionAssemblyAttributeTransformationBase(string sourcePath)
            : base(sourcePath)
        {
        }
    }
}
