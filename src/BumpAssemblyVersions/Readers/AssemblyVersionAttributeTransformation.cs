using System.Reflection;

namespace Bav
{
    internal class AssemblyVersionAttributeTransformation
        : VersionAssemblyAttributeTransformationBase<AssemblyVersionAttribute>
    {
        internal AssemblyVersionAttributeTransformation(string sourcePath)
            : base(sourcePath)
        {
        }
    }
}
